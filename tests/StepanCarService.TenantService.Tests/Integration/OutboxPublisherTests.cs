using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TenantService.Infrastructure.Messaging.Outbox;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.TenantService.Tests.Integration;

// Публикатор outbox: строгий порядок, растущая пауза при ошибках, advisory lock между инстансами, очистка старых записей.
// Сообщения заполняются напрямую в таблицу (без EfEventOutbox/TenantManagementService) — реальная сериализация payload проверена в EfEventOutboxTests;
// здесь Payload используется просто как метка "msg-N" для проверки порядка публикации
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class OutboxPublisherTests(TenantDatabase database) : DatabaseTestBase<TenantDatabase>(database)
{
    [Fact]
    public async Task PendingMessages_ArePublishedInIdOrder()
    {
        await SeedMessagesAsync(3);
        var published = RecordingMessageBus(out var messageBus);

        await using var publisher = await StartPublisherAsync(messageBus);

        await Eventually.WaitUntilAsync(() => Task.FromResult(published.Count >= 3), TimeSpan.FromSeconds(10));
        published.ShouldBe(["msg-1", "msg-2", "msg-3"]);

        await using var check = Database.CreateContext();
        var messages = await check.OutboxMessages.OrderBy(m => m.Id).ToListAsync();
        messages.ShouldAllBe(m => m.ProcessedAt != null && m.LastError == null);
    }

    [Fact]
    public async Task FirstMessageFails_LaterMessagesWaitBehindIt()
    {
        var ids = await SeedMessagesAsync(3);
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        messageBus.PublishAsync(Arg.Is("msg-1"), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new InvalidOperationException("брокер недоступен")));

        await using var publisher = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 50);

        await Eventually.WaitUntilAsync(async () =>
        {
            await using var check = Database.CreateContext();
            var first = await check.OutboxMessages.SingleAsync(m => m.Id == ids[0]);
            return first.Attempts >= 1;
        }, TimeSpan.FromSeconds(10), because: "первое сообщение должно быть отправлено с ошибкой");

        await using var db = Database.CreateContext();
        var messages = await db.OutboxMessages.OrderBy(m => m.Id).ToListAsync();
        messages[0].ProcessedAt.ShouldBeNull();
        messages[0].Attempts.ShouldBe(1);
        var lastError = messages[0].LastError;
        lastError.ShouldNotBeNull();
        lastError.ShouldContain("брокер недоступен");
        messages[0].NextAttemptAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        // Более поздние события не обгоняют застрявшее первое: публикатор останавливает пачку на первой ошибке
        messages[1].ProcessedAt.ShouldBeNull();
        messages[1].Attempts.ShouldBe(0);
        messages[2].ProcessedAt.ShouldBeNull();
        messages[2].Attempts.ShouldBe(0);
    }

    [Fact]
    public async Task RepeatedFailures_BackoffDoublesEachTime()
    {
        var ids = await SeedMessagesAsync(1);
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new InvalidOperationException("брокер недоступен")));

        await using var publisher = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 50);

        await Eventually.WaitUntilAsync(async () =>
        {
            await using var check = Database.CreateContext();
            return (await check.OutboxMessages.SingleAsync(m => m.Id == ids[0])).Attempts >= 1;
        }, TimeSpan.FromSeconds(5));
        DateTimeOffset firstNextAttemptAt;
        await using (var db = Database.CreateContext())
            firstNextAttemptAt = (await db.OutboxMessages.SingleAsync(m => m.Id == ids[0])).NextAttemptAt;
        // Первая пауза — 2 секунды (2^1)
        (firstNextAttemptAt - DateTimeOffset.UtcNow).ShouldBeInRange(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(2.5));

        await Eventually.WaitUntilAsync(async () =>
        {
            await using var check = Database.CreateContext();
            return (await check.OutboxMessages.SingleAsync(m => m.Id == ids[0])).Attempts >= 2;
        }, TimeSpan.FromSeconds(10));
        await using var final = Database.CreateContext();
        var message = await final.OutboxMessages.SingleAsync(m => m.Id == ids[0]);
        // Вторая пауза — 4 секунды (2^2), заметно больше первой
        (message.NextAttemptAt - DateTimeOffset.UtcNow).ShouldBeInRange(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4.5));
    }

    [Fact]
    public async Task MessageRecoversAfterRetryDelay_ThenQueueContinuesInOrder()
    {
        await SeedMessagesAsync(2);
        var failFirstAttempt = true;
        var published = new List<string>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var payload = callInfo.ArgAt<string>(0);
                lock (published)
                {
                    if (payload == "msg-1" && failFirstAttempt)
                    {
                        failFirstAttempt = false;
                        return Task.FromException(new InvalidOperationException("временный сбой"));
                    }
                    published.Add(payload);
                    return Task.CompletedTask;
                }
            });

        await using var publisher = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 50);

        await Eventually.WaitUntilAsync(() => Task.FromResult(published.Count >= 2), TimeSpan.FromSeconds(10),
            because: "после паузы первое сообщение должно быть отправлено успешно, за ним — второе");
        published.ShouldBe(["msg-1", "msg-2"]);
    }

    [Fact]
    public async Task BatchSizeSmallerThanQueue_PublishesAllAcrossSeveralCycles()
    {
        await SeedMessagesAsync(5);
        var published = RecordingMessageBus(out var messageBus);

        await using var publisher = await StartPublisherAsync(messageBus, batchSize: 2, pollIntervalMilliseconds: 50);

        // При BatchSize=2 нужно несколько циклов; следующая пачка берётся сразу, без ожидания PollInterval
        await Eventually.WaitUntilAsync(() => Task.FromResult(published.Count >= 5), TimeSpan.FromSeconds(5));
        published.ShouldBe(["msg-1", "msg-2", "msg-3", "msg-4", "msg-5"]);
    }

    [Fact]
    public async Task TwoPublisherInstances_PublishEachMessageExactlyOnce()
    {
        await SeedMessagesAsync(5);
        var published = RecordingMessageBus(out var messageBus);

        await using var first = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 20);
        await using var second = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 20);

        await Eventually.WaitUntilAsync(() => Task.FromResult(published.Count >= 5), TimeSpan.FromSeconds(10));
        published.Count.ShouldBe(5);
        published.Distinct().Count().ShouldBe(5);
    }

    [Fact]
    public async Task Cleanup_RemovesOnlyOldProcessedMessages()
    {
        var now = DateTimeOffset.UtcNow;
        await using (var db = Database.CreateContext())
        {
            db.OutboxMessages.AddRange(
                NewMessage("old-processed", now.AddDays(-10), processedAt: now.AddDays(-8)),
                NewMessage("recent-processed", now.AddDays(-1), processedAt: now.AddHours(-1)),
                NewMessage("still-pending", now, processedAt: null));
            await db.SaveChangesAsync();
        }
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Очистка запускается сразу на первом цикле (nextCleanupAt = "сейчас" при старте)
        await using var publisher = await StartPublisherAsync(messageBus, retentionDays: 7, pollIntervalMilliseconds: 50);

        await Eventually.WaitUntilAsync(async () =>
        {
            await using var check = Database.CreateContext();
            return await check.OutboxMessages.CountAsync() == 2;
        }, TimeSpan.FromSeconds(10), because: "старое отправленное событие должно быть удалено, остальные — остаться");

        await using var final = Database.CreateContext();
        var remainingLabels = await final.OutboxMessages.Select(m => m.Payload).ToListAsync();
        remainingLabels.ShouldNotContain("old-processed");
        remainingLabels.ShouldContain("recent-processed");
        remainingLabels.ShouldContain("still-pending");
    }

    [Fact]
    public async Task Stop_WithEmptyQueue_CompletesWithoutException()
    {
        var messageBus = Substitute.For<IMessageBus>();
        var publisher = await StartPublisherAsync(messageBus, pollIntervalMilliseconds: 50);

        await Task.Delay(TimeSpan.FromMilliseconds(200));

        await Should.NotThrowAsync(() => publisher.DisposeAsync().AsTask());
    }

    private static List<string> RecordingMessageBus(out IMessageBus messageBus)
    {
        var published = new List<string>();
        var bus = Substitute.For<IMessageBus>();
        bus.PublishAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { lock (published) published.Add(callInfo.ArgAt<string>(0)); return Task.CompletedTask; });
        messageBus = bus;
        return published;
    }

    private static OutboxMessage NewMessage(string payload, DateTimeOffset occurredAt, DateTimeOffset? processedAt) => new()
    {
        MessageId = Guid.NewGuid(),
        EventType = "TestEvent",
        Payload = payload,
        OccurredAt = occurredAt,
        NextAttemptAt = occurredAt,
        ProcessedAt = processedAt
    };

    // Заполняет очередь готовыми к отправке сообщениями с payload "msg-1".."msg-N" — по нему проверяется порядок публикации
    private async Task<List<long>> SeedMessagesAsync(int count)
    {
        await using var db = Database.CreateContext();
        var messages = Enumerable.Range(1, count)
            .Select(i => NewMessage($"msg-{i}", DateTimeOffset.UtcNow, processedAt: null))
            .ToList();
        db.OutboxMessages.AddRange(messages);
        await db.SaveChangesAsync();
        return messages.Select(m => m.Id).ToList();
    }

    private async Task<RunningPublisher> StartPublisherAsync(IMessageBus messageBus,
        int batchSize = 50, int pollIntervalMilliseconds = 200, int retentionDays = 7)
    {
        var services = new ServiceCollection();
        services.AddDbContext<TenantServiceDbContext>(o => o.UseNpgsql(Database.ConnectionString));
        var provider = services.BuildServiceProvider();

        var settings = Options.Create(new OutboxSettings
        {
            BatchSize = batchSize,
            PollIntervalMilliseconds = pollIntervalMilliseconds,
            RetentionDays = retentionDays
        });
        var publisher = new OutboxPublisher(provider.GetRequiredService<IServiceScopeFactory>(), messageBus, settings,
            NullLogger<OutboxPublisher>.Instance, TimeProvider.System);
        await publisher.StartAsync(CancellationToken.None);
        return new RunningPublisher(publisher, provider);
    }

    private sealed class RunningPublisher(OutboxPublisher publisher, ServiceProvider provider) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await publisher.StopAsync(CancellationToken.None);
            publisher.Dispose();
            await provider.DisposeAsync();
        }
    }
}
