using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using StepanCarService.Common.Application.Events;
using StepanCarService.TenantService.Infrastructure.Messaging.Outbox;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.TenantService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class EfEventOutboxTests(TenantDatabase database) : DatabaseTestBase<TenantDatabase>(database)
{
    private static readonly DateTimeOffset Now = new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Enqueue_WithoutSaveChanges_WritesNothing()
    {
        await using (var db = Database.CreateContext())
            new EfEventOutbox(db, new FakeTimeProvider(Now)).Enqueue(new TenantRegisteredEvent { Id = "t-1", Identifier = "tenant1" });

        await using var check = Database.CreateContext();
        (await check.OutboxMessages.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Enqueue_ThenSaveChanges_StoresPendingMessage()
    {
        await using (var db = Database.CreateContext())
        {
            new EfEventOutbox(db, new FakeTimeProvider(Now)).Enqueue(new TenantRegisteredEvent { Id = "t-1", Identifier = "tenant1", Name = "Сервис" });
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        var message = await check.OutboxMessages.SingleAsync();
        message.EventType.ShouldBe(nameof(TenantRegisteredEvent));
        message.Payload.ShouldContain("\"identifier\":\"tenant1\"");
        message.OccurredAt.ShouldBe(Now);
        message.NextAttemptAt.ShouldBe(Now);
        message.ProcessedAt.ShouldBeNull();
        message.MessageId.ShouldNotBe(Guid.Empty);
    }
}
