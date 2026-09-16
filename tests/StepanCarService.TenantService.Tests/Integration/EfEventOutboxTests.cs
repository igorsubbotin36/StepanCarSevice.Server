using System.Text.Json;
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

    [Theory]
    [InlineData("Updated")]
    [InlineData("Deleted")]
    public async Task Enqueue_UpdatedOrDeletedEvent_StoresCorrectEventType(string kind)
    {
        TenantEvent tenantEvent = kind == "Updated"
            ? new TenantUpdatedEvent { Id = "t-1", Identifier = "tenant1" }
            : new TenantDeletedEvent { Id = "t-1", Identifier = "tenant1" };

        await using (var db = Database.CreateContext())
        {
            new EfEventOutbox(db, new FakeTimeProvider(Now)).Enqueue(tenantEvent);
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        var message = await check.OutboxMessages.SingleAsync();
        message.EventType.ShouldBe(tenantEvent.GetType().Name);
    }

    [Fact]
    public async Task Enqueue_PassedAsBaseType_UsesRuntimeTypeForEventType()
    {
        TenantEvent tenantEvent = new TenantRegisteredEvent { Id = "t-1", Identifier = "tenant1" };

        await using (var db = Database.CreateContext())
        {
            new EfEventOutbox(db, new FakeTimeProvider(Now)).Enqueue(tenantEvent);
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        (await check.OutboxMessages.SingleAsync()).EventType.ShouldBe(nameof(TenantRegisteredEvent));
    }

    [Fact]
    public async Task Enqueue_Payload_RoundTripsAllFields()
    {
        var tenantEvent = new TenantUpdatedEvent { Id = "t-1", Identifier = "tenant1", Name = "Автосервис", IsActive = true, OwnerUserId = 7 };

        await using (var db = Database.CreateContext())
        {
            new EfEventOutbox(db, new FakeTimeProvider(Now)).Enqueue(tenantEvent);
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        var message = await check.OutboxMessages.SingleAsync();
        var deserialized = JsonSerializer.Deserialize<TenantEvent>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        deserialized.ShouldNotBeNull();
        deserialized!.EventType.ShouldBe(TenantEventType.Updated);
        deserialized.Id.ShouldBe("t-1");
        deserialized.Identifier.ShouldBe("tenant1");
        deserialized.Name.ShouldBe("Автосервис");
        deserialized.IsActive.ShouldBeTrue();
        deserialized.OwnerUserId.ShouldBe(7);
    }

    [Fact]
    public async Task Enqueue_MultipleMessagesInOneTransaction_KeepsInsertionOrderAndUniqueMessageIds()
    {
        await using (var db = Database.CreateContext())
        {
            var outbox = new EfEventOutbox(db, new FakeTimeProvider(Now));
            outbox.Enqueue(new TenantRegisteredEvent { Id = "t-1", Identifier = "tenant1" });
            outbox.Enqueue(new TenantUpdatedEvent { Id = "t-2", Identifier = "tenant2" });
            outbox.Enqueue(new TenantDeletedEvent { Id = "t-3", Identifier = "tenant3" });
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        var messages = await check.OutboxMessages.OrderBy(m => m.Id).ToListAsync();
        messages.Count.ShouldBe(3);
        messages.Select(m => m.EventType).ShouldBe([nameof(TenantRegisteredEvent), nameof(TenantUpdatedEvent), nameof(TenantDeletedEvent)]);
        messages.Select(m => m.MessageId).Distinct().Count().ShouldBe(3);
    }
}
