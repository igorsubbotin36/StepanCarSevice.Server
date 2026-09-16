using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;
using StepanCarService.TenantService.Infrastructure.Messaging;
using StepanCarService.TestKit.Messaging;

namespace StepanCarService.TenantService.Tests.Messaging;

[Trait(TestCategories.Name, TestCategories.Messaging)]
[Collection(TestCollections.RabbitMq)]
public class RabbitMQBusTests(RabbitMqVirtualHost virtualHost) : MessagingTestBase(virtualHost)
{
    private const string Payload = "{\"eventType\":0}";

    private static string UniqueExchangeName() => $"tenant.events.exchange.{Guid.NewGuid():N}";

    [Fact]
    public async Task Publish_WithBoundQueue_DeliversPersistentJsonMessage()
    {
        var exchangeName = UniqueExchangeName();
        var queue = await DeclareBoundQueueAsync(exchangeName);
        await using var bus = CreateBus(exchangeName);
        var messageId = Guid.NewGuid().ToString();

        await bus.PublishAsync(Payload, messageId);

        var message = (await WaitForMessageAsync(queue)).ShouldNotBeNull();
        Encoding.UTF8.GetString(message.Body.Span).ShouldBe(Payload);
        message.BasicProperties.Persistent.ShouldBeTrue();
        message.BasicProperties.ContentType.ShouldBe("application/json");
        message.BasicProperties.MessageId.ShouldBe(messageId);
    }

    // Публикация помечена mandatory: если ни одна очередь не привязана, сообщение не должно теряться молча
    [Fact]
    public async Task Publish_NoBoundQueue_ThrowsInsteadOfSilentlyDroppingMessage()
    {
        var exchangeName = UniqueExchangeName();
        await using var bus = CreateBus(exchangeName);

        await Should.ThrowAsync<Exception>(() => bus.PublishAsync(Payload, Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task Publish_AfterUnroutableFailure_RecoversAndDeliversNextMessage()
    {
        var exchangeName = UniqueExchangeName();
        await using var bus = CreateBus(exchangeName);
        await Should.ThrowAsync<Exception>(() => bus.PublishAsync(Payload, Guid.NewGuid().ToString()));

        // Публикатор переоткрывает соединение/канал после ошибки — следующая публикация должна пройти
        var queue = await DeclareBoundQueueAsync(exchangeName);
        var messageId = Guid.NewGuid().ToString();
        await bus.PublishAsync(Payload, messageId);

        var message = (await WaitForMessageAsync(queue)).ShouldNotBeNull();
        message.BasicProperties.MessageId.ShouldBe(messageId);
    }

    [Fact]
    public async Task ConcurrentPublishes_AllDeliveredWithoutError()
    {
        var exchangeName = UniqueExchangeName();
        var queue = await DeclareBoundQueueAsync(exchangeName);
        await using var bus = CreateBus(exchangeName);

        await Task.WhenAll(Enumerable.Range(1, 10).Select(i => bus.PublishAsync(Payload, $"concurrent-{i}")));

        await Eventually.WaitUntilAsync(async () => await GetMessageCountAsync(queue) == 10, TimeSpan.FromSeconds(10),
            because: "все параллельные публикации должны быть доставлены без потерь и без гонок на канале");
    }

    [Fact]
    public async Task Publish_DeclaresExchangeAsDurableFanout()
    {
        var exchangeName = UniqueExchangeName();
        var queue = await DeclareBoundQueueAsync(exchangeName);
        await using var bus = CreateBus(exchangeName);
        await bus.PublishAsync(Payload, Guid.NewGuid().ToString());
        (await WaitForMessageAsync(queue)).ShouldNotBeNull();

        // Повторное объявление с другими параметрами (не fanout) должно быть отклонено брокером — exchange уже существует именно как durable fanout
        await Should.ThrowAsync<OperationInterruptedException>(() =>
            Channel.ExchangeDeclareAsync(exchangeName, RabbitMQ.Client.ExchangeType.Direct, durable: true, autoDelete: false));
    }

    private RabbitMQBus CreateBus(string exchangeName) => new(Options.Create(new RabbitMQProducerSettings
    {
        HostName = VirtualHost.HostName,
        Port = VirtualHost.Port,
        UserName = VirtualHost.UserName,
        Password = VirtualHost.Password,
        VirtualHost = VirtualHost.Name,
        ExchangeName = exchangeName,
        ClientProvidedName = $"tenant-tests-{VirtualHost.Name}"
    }));
}
