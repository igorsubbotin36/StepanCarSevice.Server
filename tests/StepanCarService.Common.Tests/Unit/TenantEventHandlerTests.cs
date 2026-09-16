using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.Common.Infastructure.Messaging.Handlers;

namespace StepanCarService.Common.Tests.Unit;

// Идемпотентный обработчик событий тенанта: применяет upsert/delete к локальной копии сервиса
[Trait(TestCategories.Name, TestCategories.Unit)]
public class TenantEventHandlerTests
{
    private readonly ITenantRepository _repository = Substitute.For<ITenantRepository>();
    private readonly TenantEventHandler<ITenantRepository> _handler;

    public TenantEventHandlerTests()
    {
        _handler = new TenantEventHandler<ITenantRepository>(_repository, NullLogger<TenantEventHandler<ITenantRepository>>.Instance);
    }

    [Fact]
    public async Task HandleAsync_NullEvent_ReturnsFailureWithoutTouchingRepository()
    {
        var result = await _handler.HandleAsync(null!);

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(ModelErrors.RequestedModelIsNull);
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!);
    }

    [Fact]
    public async Task HandleAsync_EventWithoutId_ReturnsFailure()
    {
        var tenantEvent = new TenantRegisteredEvent { Id = "  ", Identifier = "tenant1" };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(ModelErrors.RequestedModelIsNull);
    }

    [Theory]
    [InlineData(TenantEventType.Registered)]
    [InlineData(TenantEventType.Updated)]
    public async Task HandleAsync_UpsertWithoutIdentifier_ReturnsFailure(TenantEventType eventType)
    {
        var tenantEvent = new TenantEvent { EventType = eventType, Id = "tenant-1", Identifier = "" };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(TenantErrors.InvalidIdentifier);
    }

    [Fact]
    public async Task HandleAsync_RegisteredForNewTenant_AddsTenantWithAllFields()
    {
        _repository.GetByIdAsync("tenant-1").Returns((TenantInfoEntity?)null);
        var tenantEvent = new TenantRegisteredEvent
        {
            Id = "tenant-1",
            Identifier = "tenant1",
            Name = "Автосервис",
            IsActive = true,
            OwnerUserId = 7
        };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Is<TenantInfoEntity>(t =>
            t.Id == "tenant-1" && t.Identifier == "tenant1" && t.Name == "Автосервис" && t.IsActive && t.OwnerUserId == 7));
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
    }

    [Fact]
    public async Task HandleAsync_RegisteredForExistingTenant_UpdatesInsteadOfDuplicating()
    {
        var existing = new TenantInfoEntity { Id = "tenant-1", Identifier = "old", Name = "Старое имя", IsActive = false };
        _repository.GetByIdAsync("tenant-1").Returns(existing);
        var tenantEvent = new TenantRegisteredEvent { Id = "tenant-1", Identifier = "tenant1", Name = "Новое имя", IsActive = true };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(Arg.Is<TenantInfoEntity>(t =>
            t.Id == "tenant-1" && t.Identifier == "tenant1" && t.Name == "Новое имя" && t.IsActive));
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!);
    }

    [Fact]
    public async Task HandleAsync_UpdatedForMissingTenant_CreatesIt()
    {
        _repository.GetByIdAsync("tenant-1").Returns((TenantInfoEntity?)null);
        var tenantEvent = new TenantUpdatedEvent { Id = "tenant-1", Identifier = "tenant1", Name = "Сервис", IsActive = true };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Is<TenantInfoEntity>(t => t.Id == "tenant-1"));
    }

    [Fact]
    public async Task HandleAsync_DeletedForExistingTenant_DeletesIt()
    {
        var existing = new TenantInfoEntity { Id = "tenant-1", Identifier = "tenant1" };
        _repository.GetByIdAsync("tenant-1").Returns(existing);
        var tenantEvent = new TenantDeletedEvent { Id = "tenant-1", Identifier = "tenant1" };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(existing);
    }

    [Fact]
    public async Task HandleAsync_DeletedForMissingTenant_IsSuccessWithoutCallingDelete()
    {
        _repository.GetByIdAsync("tenant-1").Returns((TenantInfoEntity?)null);
        var tenantEvent = new TenantDeletedEvent { Id = "tenant-1", Identifier = "tenant1" };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeTrue();
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!);
    }

    [Fact]
    public async Task HandleAsync_UnknownEventType_ReturnsFailure()
    {
        var tenantEvent = new TenantEvent { EventType = (TenantEventType)99, Id = "tenant-1", Identifier = "tenant1" };

        var result = await _handler.HandleAsync(tenantEvent);

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(SystemErrors.InternalError);
    }

    [Fact]
    public async Task HandleAsync_DatabaseErrorOnAdd_ExceptionPropagates()
    {
        _repository.GetByIdAsync("tenant-1").Returns((TenantInfoEntity?)null);
        _repository.AddAsync(Arg.Any<TenantInfoEntity>()).Returns(Task.FromException(new InvalidOperationException("БД недоступна")));
        var tenantEvent = new TenantRegisteredEvent { Id = "tenant-1", Identifier = "tenant1" };

        await Should.ThrowAsync<InvalidOperationException>(() => _handler.HandleAsync(tenantEvent));
    }
}
