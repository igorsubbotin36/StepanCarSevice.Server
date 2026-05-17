using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StepanCarService.Core.Models;
using StepanCarService.TenantService.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace StepanCarService.TenantService.Infrastructure.Messaging
{
    public class RabbitMQBus : IMessageBus, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMQProducerSettings _settings;
        private bool _disposed;
        public RabbitMQBus(IOptions<RabbitMQProducerSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
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
        public async Task<Result> PublishAsync<T>(T message) where T : class
        {
            try
            {
                await _channel.ExchangeDeclareAsync(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var body = Encoding.UTF8.GetBytes(json);
                var properties = new BasicProperties()
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await _channel.BasicPublishAsync(
                    exchange: _settings.ExchangeName,
                    routingKey: null,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(MessageBusErrors.MessageNotDelivered);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _disposed = true;
        }
    }
}
