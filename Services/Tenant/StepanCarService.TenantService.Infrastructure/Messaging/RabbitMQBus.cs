using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StepanCarService.TenantService.Application.Interfaces;
using System.Text;

namespace StepanCarService.TenantService.Infrastructure.Messaging
{
    // Публикация в fanout exchange с подтверждением брокера (publisher confirms).
    // Соединение создаётся при первой публикации и пересоздаётся после ошибки,
    // поэтому недоступный при старте RabbitMQ не роняет сервис
    public class RabbitMQBus : IMessageBus, IAsyncDisposable
    {
        private readonly RabbitMQProducerSettings _settings;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQBus(IOptions<RabbitMQProducerSettings> options)
        {
            _settings = options.Value;
        }

        public async Task PublishAsync(string payload, string messageId, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var channel = await GetChannelAsync(cancellationToken);
                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = messageId,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };
                // mandatory: если сообщение некуда доставить (нет ни одной очереди), публикация завершится ошибкой,
                // и outbox повторит её позже, а не потеряет событие
                await channel.BasicPublishAsync(
                    exchange: _settings.ExchangeName,
                    routingKey: string.Empty,
                    mandatory: true,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(payload),
                    cancellationToken: cancellationToken);
            }
            catch
            {
                await ResetAsync();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            await ResetAsync();
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                ClientProvidedName = _settings.ClientProvidedName
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                cancellationToken);
            await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Fanout,
                durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            return _channel;
        }

        private async Task ResetAsync()
        {
            try
            {
                if (_channel != null) await _channel.DisposeAsync();
                if (_connection != null) await _connection.DisposeAsync();
            }
            catch
            {
                // Соединение уже разорвано — пересоздадим при следующей публикации
            }
            finally
            {
                _channel = null;
                _connection = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await ResetAsync();
            _lock.Dispose();
        }
    }
}
