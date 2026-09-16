using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

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
    // Обычный канал; выделен отдельно от Channel, чтобы попытка обратиться к ещё не созданной очереди
    // (например, фоновым consumer'ом) не закрывала общий канал теста по правилам AMQP
    private IChannel PollingChannel { get; set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        var factory = await VirtualHost.CreateConnectionFactoryAsync();
        Connection = await factory.CreateConnectionAsync();
        Channel = await Connection.CreateChannelAsync();
        PollingChannel = await Connection.CreateChannelAsync();
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

    // Ждёт сообщение в очереди (опрос BasicGet); null — не пришло за timeout.
    // Если очередь ещё не объявлена (например, фоновым consumer'ом при подключении), AMQP закрывает канал
    // ошибкой NOT_FOUND — переоткрываем отдельный опрашивающий канал и пробуем снова на следующем шаге опроса
    protected async Task<BasicGetResult?> WaitForMessageAsync(string queueName, TimeSpan? timeout = null)
    {
        BasicGetResult? result = null;
        await Eventually.WaitUntilAsync(async () =>
        {
            try
            {
                result = await PollingChannel.BasicGetAsync(queueName, autoAck: true);
                return result != null;
            }
            catch (OperationInterruptedException)
            {
                PollingChannel = await Connection.CreateChannelAsync();
                return false;
            }
        }, timeout ?? TimeSpan.FromSeconds(10), throwOnTimeout: false);
        return result;
    }

    protected async Task<uint> GetMessageCountAsync(string queueName)
    {
        try
        {
            return (await PollingChannel.QueueDeclarePassiveAsync(queueName)).MessageCount;
        }
        catch (OperationInterruptedException)
        {
            PollingChannel = await Connection.CreateChannelAsync();
            throw;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
        await PollingChannel.DisposeAsync();
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
