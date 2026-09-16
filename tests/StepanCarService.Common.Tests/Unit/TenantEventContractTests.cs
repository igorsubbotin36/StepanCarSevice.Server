using System.Text.Json;
using System.Text.Json.Nodes;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Core.Entities;

namespace StepanCarService.Common.Tests.Unit;

// Контракт сообщения о тенанте между публикатором (Tenant, EfEventOutbox) и получателями (TenantEventsConsumer)
[Trait(TestCategories.Name, TestCategories.Unit)]
public class TenantEventContractTests
{
    // Как пишет EfEventOutbox: camelCase и runtime-тип события
    private static readonly JsonSerializerOptions PublisherOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    // Как читает TenantEventsConsumer
    private static readonly JsonSerializerOptions ConsumerOptions = new() { PropertyNameCaseInsensitive = true };

    private static string Publish(TenantEvent tenantEvent) =>
        JsonSerializer.Serialize(tenantEvent, tenantEvent.GetType(), PublisherOptions);

    [Fact]
    public void RegisteredEvent_RoundTripsFromPublisherToConsumer()
    {
        var published = new TenantRegisteredEvent
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = "tenant1",
            Name = "Автосервис",
            IsActive = true,
            OwnerUserId = 7
        };

        var received = JsonSerializer.Deserialize<TenantEvent>(Publish(published), ConsumerOptions);

        received.ShouldNotBeNull();
        received.EventType.ShouldBe(TenantEventType.Registered);
        received.Id.ShouldBe(published.Id);
        received.Identifier.ShouldBe(published.Identifier);
        received.Name.ShouldBe(published.Name);
        received.IsActive.ShouldBeTrue();
        received.OwnerUserId.ShouldBe(7);
    }

    // Значения enum — часть контракта; перестановка сломает уже лежащие в очередях сообщения
    [Theory]
    [InlineData(typeof(TenantRegisteredEvent), 0)]
    [InlineData(typeof(TenantUpdatedEvent), 1)]
    [InlineData(typeof(TenantDeletedEvent), 2)]
    public void EventType_IsSerializedAsStableNumber(Type eventType, int expected)
    {
        var tenantEvent = (TenantEvent)Activator.CreateInstance(eventType)!;

        var json = JsonNode.Parse(Publish(tenantEvent))!;

        json["eventType"]!.GetValueKind().ShouldBe(JsonValueKind.Number);
        json["eventType"]!.GetValue<int>().ShouldBe(expected);
    }

    // Сообщения старого формата (до удаления секретов из события) ещё могут лежать в очередях
    [Fact]
    public void OldMessageWithRemovedFields_IsDeserialized()
    {
        const string json = """
            {"eventType":1,"id":"t-1","identifier":"tenant1","name":"Сервис","isActive":false,
             "apiKey":"secret","connectionString":"Host=db;Password=secret"}
            """;

        var received = JsonSerializer.Deserialize<TenantEvent>(json, ConsumerOptions);

        received.ShouldNotBeNull();
        received.EventType.ShouldBe(TenantEventType.Updated);
        received.Identifier.ShouldBe("tenant1");
        received.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Payload_ContainsOnlyPublicTenantFields()
    {
        var json = JsonNode.Parse(Publish(new TenantUpdatedEvent { Id = "t-1", Identifier = "tenant1", Name = "Сервис" }))!.AsObject();

        json.Select(p => p.Key).OrderBy(k => k)
            .ShouldBe(["eventType", "id", "identifier", "isActive", "name", "ownerUserId"]);
    }

    [Fact]
    public void TenantEvent_DoesNotInheritTenantEntity()
    {
        typeof(TenantInfoEntity).IsAssignableFrom(typeof(TenantEvent)).ShouldBeFalse();
        typeof(TenantEvent).BaseType.ShouldBe(typeof(object));
    }
}
