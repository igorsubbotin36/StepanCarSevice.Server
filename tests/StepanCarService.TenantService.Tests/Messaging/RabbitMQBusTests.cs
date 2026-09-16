using System.Text;
using Microsoft.Extensions.Options;
using StepanCarService.TenantService.Infrastructure.Messaging;
using StepanCarService.TestKit.Messaging;

namespace StepanCarService.TenantService.Tests.Messaging;

[Trait(TestCategories.Name, TestCategories.Messaging)]
[Collection(TestCollections.RabbitMq)]
public class RabbitMQBusTests(RabbitMqVirtualHost virtualHost) : MessagingTestBase(virtualHost)
{
    private const string ExchangeName = "tenant.events.exchange";
    private const string Payload = "{\"eventType\":0}";

    // TN-60
    [Fact]
    public async Task Publish_WithBoundQueue_DeliversPersistentJsonMessage()
    {
        var queue = await DeclareBoundQueueAsync(ExchangeName);
        await using var bus = CreateBus();
        var messageId = Guid.NewGuid().ToString();

        await bus.PublishAsync(Payload, messageId);

        var message = (await WaitForMessageAsync(queue)).ShouldNotBeNull();
        Encoding.UTF8.GetString(message.Body.Span).ShouldBe(Payload);
        message.BasicProperties.Persistent.ShouldBeTrue();
        message.BasicProperties.ContentType.ShouldBe("application/json");
        message.BasicProperties.MessageId.ShouldBe(messageId);
    }

    private RabbitMQBus CreateBus() => new(Options.Create(new RabbitMQProducerSettings
    {
        HostName = VirtualHost.HostName,
        Port = VirtualHost.Port,
        UserName = VirtualHost.UserName,
        Password = VirtualHost.Password,
        VirtualHost = VirtualHost.Name,
        ExchangeName = ExchangeName,
        ClientProvidedName = $"tenant-tests-{VirtualHost.Name}"
    }));
}
