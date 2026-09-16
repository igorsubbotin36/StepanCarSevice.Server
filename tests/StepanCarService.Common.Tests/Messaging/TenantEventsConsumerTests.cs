using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Infastructure.Messaging;
using StepanCarService.TestKit.Messaging;

namespace StepanCarService.Common.Tests.Messaging;

[Trait(TestCategories.Name, TestCategories.Messaging)]
[Collection(TestCollections.RabbitMq)]
public class TenantEventsConsumerTests(RabbitMqVirtualHost virtualHost) : MessagingTestBase(virtualHost)
{
    private const string ExchangeName = "tenant.events.exchange";
    private const string QueueName = "common.tests.tenant.events";

    [Fact]
    public async Task ValidEvent_IsHandledAndAcknowledged()
    {
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>()).Returns(Result.Success());
        // Очередь объявляется заранее с теми же параметрами, что у consumer'а, чтобы сообщение не потерялось до подписки
        await DeclareBoundQueueAsync(ExchangeName, QueueName);
        await PublishAsync(ExchangeName, """{"eventType":0,"id":"tenant-1","identifier":"tenant1","name":"Сервис","isActive":true}""");

        await using var consumer = await StartConsumerAsync(handler);

        await Eventually.WaitUntilAsync(async () => handler.ReceivedCalls().Any() && await GetMessageCountAsync(QueueName) == 0,
            TimeSpan.FromSeconds(15), because: "событие должно быть обработано и подтверждено");
        await handler.Received(1).HandleAsync(Arg.Is<TenantEvent>(e =>
            e.EventType == TenantEventType.Registered && e.Id == "tenant-1" && e.Identifier == "tenant1"));
        (await GetMessageCountAsync($"{QueueName}.dead")).ShouldBe(0u);
    }

    private async Task<RunningConsumer> StartConsumerAsync(ITenantEventHandler handler)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => handler);
        var provider = services.BuildServiceProvider();

        var settings = Options.Create(new RabbitMQConsumerSetting
        {
            HostName = VirtualHost.HostName,
            Port = VirtualHost.Port,
            UserName = VirtualHost.UserName,
            Password = VirtualHost.Password,
            VirtualHost = VirtualHost.Name,
            ExchangeName = ExchangeName,
            QueueName = QueueName
        });
        var consumer = new TenantEventsConsumer(settings, provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<TenantEventsConsumer>.Instance, TimeProvider.System);
        await consumer.StartAsync(CancellationToken.None);
        return new RunningConsumer(consumer, provider);
    }

    private sealed class RunningConsumer(TenantEventsConsumer consumer, ServiceProvider provider) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await consumer.StopAsync(CancellationToken.None);
            consumer.Dispose();
            await provider.DisposeAsync();
        }
    }
}
