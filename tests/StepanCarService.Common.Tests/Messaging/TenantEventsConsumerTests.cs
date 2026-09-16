using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
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
    private const string QueueBaseName = "common.tests.tenant.events";

    private static string UniqueQueueName() => $"{QueueBaseName}.{Guid.NewGuid():N}";
    private static string RegisteredPayload(string id) =>
        $$"""{"eventType":0,"id":"{{id}}","identifier":"tenant1","name":"Сервис","isActive":true}""";

    [Fact]
    public async Task ValidEvent_IsHandledAndAcknowledged()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>()).Returns(Result.Success());
        // Очередь объявляется заранее с теми же параметрами, что у consumer'а, чтобы сообщение не потерялось до подписки
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-1"));

        await using var consumer = await StartConsumerAsync(handler, queueName);

        await Eventually.WaitUntilAsync(async () => handler.ReceivedCalls().Any() && await GetMessageCountAsync(queueName) == 0,
            TimeSpan.FromSeconds(15), because: "событие должно быть обработано и подтверждено");
        await handler.Received(1).HandleAsync(Arg.Is<TenantEvent>(e =>
            e.EventType == TenantEventType.Registered && e.Id == "tenant-1" && e.Identifier == "tenant1"));
        (await GetMessageCountAsync($"{queueName}.dead")).ShouldBe(0u);
    }

    [Fact]
    public async Task Consumer_DeclaresQueueAndDeadLetterQueueEagerly()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();

        await using var consumer = await StartConsumerAsync(handler, queueName);

        // Очередь и очередь "мертвых" сообщений объявлены сразу при подписке, ещё до первого сбоя
        await Eventually.WaitUntilAsync(async () =>
        {
            try { await GetMessageCountAsync($"{queueName}.dead"); return true; }
            catch { return false; }
        }, TimeSpan.FromSeconds(10), because: "очередь мертвых сообщений должна быть объявлена заранее");
        (await GetMessageCountAsync(queueName)).ShouldBe(0u);
        (await GetMessageCountAsync($"{queueName}.dead")).ShouldBe(0u);
    }

    [Fact]
    public async Task MalformedJson_GoesToDeadLetterWithoutRetry()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        const string malformedBody = "{not-a-valid-json";
        await PublishAsync(ExchangeName, malformedBody, messageId: "msg-malformed");

        await using var consumer = await StartConsumerAsync(handler, queueName);

        var dead = await WaitForMessageAsync($"{queueName}.dead");
        dead.ShouldNotBeNull();
        await handler.DidNotReceiveWithAnyArgs().HandleAsync(default!);
        (await GetMessageCountAsync(queueName)).ShouldBe(0u);
        Encoding.UTF8.GetString(dead!.Body.ToArray()).ShouldBe(malformedBody);
        dead.BasicProperties.MessageId.ShouldBe("msg-malformed");
        HeaderNumber(dead.BasicProperties.Headers!["x-attempts"]!).ShouldBe(1);
        HeaderText(dead.BasicProperties.Headers!["x-error"]!).ShouldContain("JSON");
        HeaderText(dead.BasicProperties.Headers!["x-original-queue"]!).ShouldBe(queueName);
    }

    [Fact]
    public async Task HandlerRejectsEvent_GoesToDeadLetterImmediately()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>()).Returns(Result.Failure(ModelErrors.ModelNotFound));
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-2"));

        await using var consumer = await StartConsumerAsync(handler, queueName);

        var dead = await WaitForMessageAsync($"{queueName}.dead");
        dead.ShouldNotBeNull();
        await handler.Received(1).HandleAsync(Arg.Any<TenantEvent>());
        HeaderNumber(dead!.BasicProperties.Headers!["x-attempts"]!).ShouldBe(1);
        HeaderText(dead.BasicProperties.Headers!["x-error"]!).ShouldContain(ModelErrors.ModelNotFound);
    }

    [Fact]
    public async Task PersistentError_IsRetriedUpToLimitThenDeadLettered()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>())
            .Returns(_ => Task.FromException<Result>(new InvalidOperationException("постоянная ошибка")));
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-3"));

        await using var consumer = await StartConsumerAsync(handler, queueName,
            maxFailedAttempts: 3, retryBaseDelayMilliseconds: 50, retryMaxDelayMilliseconds: 200);

        var dead = await WaitForMessageAsync($"{queueName}.dead", TimeSpan.FromSeconds(15));
        dead.ShouldNotBeNull();
        handler.ReceivedCalls().Count().ShouldBe(3);
        HeaderNumber(dead!.BasicProperties.Headers!["x-attempts"]!).ShouldBe(3);
    }

    [Fact]
    public async Task TransientError_RetriesWithoutLimitUntilSuccess()
    {
        var queueName = UniqueQueueName();
        var attempts = 0;
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>()).Returns(_ =>
        {
            attempts++;
            // Больше неудачных попыток, чем разрешает MaxFailedAttempts, — но ошибка временная, лимит на неё не действует
            if (attempts < 4)
                return Task.FromException<Result>(new NpgsqlException("connection lost", new SocketException((int)SocketError.ConnectionReset)));
            return Task.FromResult(Result.Success());
        });
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-4"));

        await using var consumer = await StartConsumerAsync(handler, queueName,
            maxFailedAttempts: 2, retryBaseDelayMilliseconds: 50, retryMaxDelayMilliseconds: 200);

        await Eventually.WaitUntilAsync(() => Task.FromResult(attempts >= 4), TimeSpan.FromSeconds(15),
            because: "временная ошибка должна повторяться, пока не пройдёт, несмотря на MaxFailedAttempts");
        (await GetMessageCountAsync($"{queueName}.dead")).ShouldBe(0u);
    }

    [Fact]
    public async Task SecondMessage_WaitsUntilFirstMessageIsResolved()
    {
        var queueName = UniqueQueueName();
        var order = new List<string>();
        var firstAttempt = 0;
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>()).Returns(callInfo =>
        {
            var tenantEvent = callInfo.Arg<TenantEvent>();
            lock (order)
            {
                if (tenantEvent.Id == "tenant-first")
                {
                    firstAttempt++;
                    order.Add($"first-{firstAttempt}");
                    if (firstAttempt < 2)
                        return Task.FromException<Result>(new InvalidOperationException("первая попытка не удалась"));
                    return Task.FromResult(Result.Success());
                }
                order.Add("second-1");
                return Task.FromResult(Result.Success());
            }
        });
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-first"));
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-second"));

        await using var consumer = await StartConsumerAsync(handler, queueName,
            maxFailedAttempts: 5, retryBaseDelayMilliseconds: 200, retryMaxDelayMilliseconds: 500);

        await Eventually.WaitUntilAsync(() => Task.FromResult(order.Count >= 3), TimeSpan.FromSeconds(15),
            because: "оба сообщения в итоге должны быть обработаны");
        // Второе сообщение не должно обрабатываться, пока первое не будет разрешено (успех или dead-letter)
        order.ShouldBe(["first-1", "first-2", "second-1"]);
    }

    [Fact]
    public async Task Stop_DuringRetryDelay_RequeuesMessageInsteadOfDeadLettering()
    {
        var queueName = UniqueQueueName();
        var handler = Substitute.For<ITenantEventHandler>();
        handler.HandleAsync(Arg.Any<TenantEvent>())
            .Returns(_ => Task.FromException<Result>(new InvalidOperationException("не удалось обработать")));
        await DeclareBoundQueueAsync(ExchangeName, queueName);
        await PublishAsync(ExchangeName, RegisteredPayload("tenant-5"), messageId: "msg-requeue");

        var consumer = await StartConsumerAsync(handler, queueName,
            maxFailedAttempts: 5, retryBaseDelayMilliseconds: 30_000, retryMaxDelayMilliseconds: 60_000);
        try
        {
            // Первая попытка уже случилась и потребитель ушёл в долгую паузу перед повтором
            await Eventually.WaitUntilAsync(() => Task.FromResult(handler.ReceivedCalls().Count() >= 1), TimeSpan.FromSeconds(10));
        }
        finally
        {
            await consumer.Consumer.StopAsync(CancellationToken.None);
            consumer.Consumer.Dispose();
            await consumer.Provider.DisposeAsync();
        }

        var requeued = await WaitForMessageAsync(queueName, TimeSpan.FromSeconds(10));
        requeued.ShouldNotBeNull();
        requeued!.BasicProperties.MessageId.ShouldBe("msg-requeue");
        (await GetMessageCountAsync($"{queueName}.dead")).ShouldBe(0u);
    }

    private static long HeaderNumber(object value) => Convert.ToInt64(value);

    private static string HeaderText(object value) => value switch
    {
        string s => s,
        byte[] bytes => Encoding.UTF8.GetString(bytes),
        _ => value.ToString()!
    };

    private async Task<RunningConsumer> StartConsumerAsync(ITenantEventHandler handler, string queueName,
        int maxFailedAttempts = 5, int retryBaseDelayMilliseconds = 1000, int retryMaxDelayMilliseconds = 60_000)
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
            QueueName = queueName,
            MaxFailedAttempts = maxFailedAttempts,
            RetryBaseDelayMilliseconds = retryBaseDelayMilliseconds,
            RetryMaxDelayMilliseconds = retryMaxDelayMilliseconds
        });
        var consumer = new TenantEventsConsumer(settings, provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<TenantEventsConsumer>.Instance, TimeProvider.System);
        await consumer.StartAsync(CancellationToken.None);
        return new RunningConsumer(consumer, provider);
    }

    private sealed class RunningConsumer(TenantEventsConsumer consumer, ServiceProvider provider) : IAsyncDisposable
    {
        public TenantEventsConsumer Consumer => consumer;
        public ServiceProvider Provider => provider;

        public async ValueTask DisposeAsync()
        {
            await consumer.StopAsync(CancellationToken.None);
            consumer.Dispose();
            await provider.DisposeAsync();
        }
    }
}
