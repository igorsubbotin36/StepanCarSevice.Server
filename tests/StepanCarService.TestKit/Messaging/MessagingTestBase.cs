using System.Text;
using RabbitMQ.Client;

namespace StepanCarService.TestKit.Messaging;

// Базовый класс messaging-тестов: свой vhost на класс, соединение на тест и помощники для очередей
public abstract class MessagingTestBase : IClassFixture<RabbitMqVirtualHost>, IAsyncLifetime
{
    protected MessagingTestBase(RabbitMqVirtualHost virtualHost)
    {
        VirtualHost = virtualHost;
    }

    protected RabbitMqVirtualHost VirtualHost { get; }
    protected IConnection Connection { get; private set; } = null!;
    protected IChannel Channel { get; private set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        var factory = await VirtualHost.CreateConnectionFactoryAsync();
        Connection = await factory.CreateConnectionAsync();
        Channel = await Connection.CreateChannelAsync();
    }

    // Очередь, привязанная к fanout exchange: получает всё, что публикуется в exchange
    protected async Task<string> DeclareBoundQueueAsync(string exchangeName, string? queueName = null)
    {
        await Channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
        var queue = await Channel.QueueDeclareAsync(queueName ?? $"test.{Guid.NewGuid():N}",
            durable: true, exclusive: false, autoDelete: false);
        await Channel.QueueBindAsync(queue.QueueName, exchangeName, routingKey: string.Empty);
        return queue.QueueName;
    }

    protected async Task PublishAsync(string exchangeName, string body, string? messageId = null, string routingKey = "")
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = messageId ?? Guid.NewGuid().ToString()
        };
        await Channel.BasicPublishAsync(exchangeName, routingKey, mandatory: false, properties, Encoding.UTF8.GetBytes(body));
    }

    // Ждёт сообщение в очереди (опрос BasicGet); null — не пришло за timeout
    protected async Task<BasicGetResult?> WaitForMessageAsync(string queueName, TimeSpan? timeout = null)
    {
        BasicGetResult? result = null;
        await Eventually.WaitUntilAsync(async () =>
        {
            result = await Channel.BasicGetAsync(queueName, autoAck: true);
            return result != null;
        }, timeout ?? TimeSpan.FromSeconds(10), throwOnTimeout: false);
        return result;
    }

    protected async Task<uint> GetMessageCountAsync(string queueName) =>
        (await Channel.QueueDeclarePassiveAsync(queueName)).MessageCount;

    public virtual async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
