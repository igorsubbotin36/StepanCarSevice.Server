using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Interfaces;

namespace StepanCarService.Common.Infastructure.Messaging
{
    // Получает события о тенантах и применяет их к копии тенантов сервиса.
    //  - Ack только после успешной обработки: событие не теряется при ошибке.
    //  - Временные ошибки (БД/сеть недоступны) повторяются с растущей паузой, пока не пройдут;
    //    очередь при этом ждёт, порядок событий сохраняется.
    //  - Некорректные сообщения и постоянные ошибки после MaxFailedAttempts уходят в очередь "<QueueName>.dead".
    //  - Недоступный при старте RabbitMQ не роняет сервис: подключение повторяется в фоне.
    public class TenantEventsConsumer : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQConsumerSetting _settings;
        private readonly ILogger<TenantEventsConsumer> _logger;
        private readonly TimeProvider _timeProvider;
        private IConnection? _connection;
        private IChannel? _channel;
        private CancellationToken _stoppingToken;

        public TenantEventsConsumer(IOptions<RabbitMQConsumerSetting> options,
            IServiceScopeFactory scopeFactory,
            ILogger<TenantEventsConsumer> logger,
            TimeProvider timeProvider)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _timeProvider = timeProvider;
        }

        private string DeadLetterQueue => $"{_settings.QueueName}.dead";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            var delay = TimeSpan.FromSeconds(1);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndConsumeAsync(stoppingToken);
                    _logger.LogInformation($"RabbitMQ: подписка на очередь {_settings.QueueName} запущена");
                    // Дальнейшие обрывы связи обрабатывает автоматическое восстановление RabbitMQ.Client
                    return;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"RabbitMQ недоступен ({e.Message}). Повторное подключение через {delay.TotalSeconds:0} с");
                    await CloseAsync();
                    try { await Task.Delay(delay, _timeProvider, stoppingToken); } catch (OperationCanceledException) { return; }
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
                }
            }
        }

        private async Task ConnectAndConsumeAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = _settings.QueueName
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Fanout,
                durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(_settings.QueueName,
                durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName,
                routingKey: string.Empty, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(DeadLetterQueue,
                durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _settings.PrefetchCount, global: false, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageAsync;
            await _channel.BasicConsumeAsync(_settings.QueueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        }

        private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            var channel = _channel!;
            var body = ea.Body.ToArray();
            try
            {
                TenantEvent? tenantEvent;
                try
                {
                    tenantEvent = JsonSerializer.Deserialize<TenantEvent>(Encoding.UTF8.GetString(body), JsonOptions);
                }
                catch (JsonException e)
                {
                    await DeadLetterAsync(channel, ea, body, $"Некорректный JSON: {e.Message}", attempts: 1);
                    return;
                }

                var attempt = 0;
                var delay = TimeSpan.FromMilliseconds(_settings.RetryBaseDelayMilliseconds);
                var maxDelay = TimeSpan.FromMilliseconds(_settings.RetryMaxDelayMilliseconds);
                while (true)
                {
                    attempt++;
                    try
                    {
                        // Новый scope на каждую попытку: свежий DbContext без состояния прошлой ошибки
                        using var scope = _scopeFactory.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<ITenantEventHandler>();
                        var result = await handler.HandleAsync(tenantEvent!);
                        if (result.IsSuccess)
                        {
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            // Сообщение некорректно — повтор не поможет
                            await DeadLetterAsync(channel, ea, body, $"Событие отклонено обработчиком: {result.ErrorCode}", attempt);
                        }
                        return;
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        var transient = IsTransient(e);
                        if (!transient && attempt >= _settings.MaxFailedAttempts)
                        {
                            await DeadLetterAsync(channel, ea, body, e.ToString(), attempt);
                            return;
                        }
                        _logger.LogWarning($"Событие тенанта не обработано (попытка {attempt}, {(transient ? "временная ошибка" : "ошибка")}): {e.Message}. Повтор через {delay.TotalSeconds:0} с");
                        await Task.Delay(delay, _timeProvider, _stoppingToken);
                        delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, maxDelay.Ticks));
                    }
                }
            }
            catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
            {
                // Сервис останавливается: вернуть сообщение в очередь, чтобы обработать после запуска
                await TryNackAsync(channel, ea.DeliveryTag);
            }
            catch (Exception e)
            {
                // Например, не удалось отправить в очередь «.dead»: сообщение остаётся в основной очереди
                _logger.LogError($"Ошибка обработки сообщения RabbitMQ, сообщение возвращено в очередь\n{e}");
                await TryNackAsync(channel, ea.DeliveryTag);
            }
        }

        private async Task DeadLetterAsync(IChannel channel, BasicDeliverEventArgs ea, byte[] body, string reason, int attempts)
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = ea.BasicProperties.ContentType,
                MessageId = ea.BasicProperties.MessageId,
                Headers = new Dictionary<string, object?>
                {
                    ["x-original-queue"] = _settings.QueueName,
                    ["x-failed-at"] = _timeProvider.GetUtcNow().ToString("O"),
                    ["x-attempts"] = attempts,
                    ["x-error"] = reason.Length > 2000 ? reason[..2000] : reason
                }
            };
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: DeadLetterQueue,
                mandatory: false, basicProperties: properties, body: body);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            _logger.LogError($"Событие тенанта перемещено в очередь {DeadLetterQueue} (попыток: {attempts}): {reason}");
        }

        private async Task TryNackAsync(IChannel channel, ulong deliveryTag)
        {
            try
            {
                await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
            }
            catch (Exception e)
            {
                // Канал закрыт: неподтверждённое сообщение брокер вернёт в очередь сам
                _logger.LogWarning($"Не удалось вернуть сообщение в очередь: {e.Message}");
            }
        }

        // Временная ошибка — БД или сеть недоступны; такие события ждут восстановления без ограничения попыток
        internal static bool IsTransient(Exception exception)
        {
            for (var e = exception; e != null; e = e.InnerException)
            {
                if (e is NpgsqlException { IsTransient: true } or TimeoutException or SocketException)
                    return true;
            }
            return false;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await CloseAsync();
        }

        private async Task CloseAsync()
        {
            try
            {
                if (_channel != null) await _channel.DisposeAsync();
                if (_connection != null) await _connection.DisposeAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Ошибка при закрытии соединения RabbitMQ: {e.Message}");
            }
            finally
            {
                _channel = null;
                _connection = null;
            }
        }
    }
}
