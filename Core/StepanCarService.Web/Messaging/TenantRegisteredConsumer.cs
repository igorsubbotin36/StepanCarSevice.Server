using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StepanCarService.Core.Events;
using StepanCarSevice.AuthService.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace StepanCarService.Web.Messaging
{
    public class TenantRegisteredConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMQConsumerSetting _settings;
        public TenantRegisteredConsumer(IOptions<RabbitMQConsumerSetting> options, IServiceScopeFactory scopeFactory)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                if (_connection == null)
                    throw new Exception("Failed to create connection");
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                if (_channel == null)
                    throw new Exception("Failed to create channel");
            }
            catch (Exception ex)
            {
                throw new Exception($"RabbitMQ connection failed: {ex.Message}", ex);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Exchange: '{_settings.ExchangeName}', Queue: '{_settings.QueueName}'");
            await _channel.ExchangeDeclareAsync(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
            await _channel.QueueDeclareAsync(
                   queue: _settings.QueueName,
                   durable: true,
                   exclusive: false,
                   autoDelete: false);
            await _channel.QueueBindAsync(
                    _settings.QueueName,
                    _settings.ExchangeName,
                    routingKey: null
                    );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ITenantRegisteredHandler>();

                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var tenantEvent = JsonSerializer.Deserialize<TenantRegisteredEvent>(message, options);
                    await handler.HandleAsync(tenantEvent);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };
            await _channel.BasicConsumeAsync(_settings.QueueName, false, consumer);
        }
    }
}
