using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Models.DTOs;
using System.Text.RegularExpressions;

namespace StepanCarService.TenantService.Application.Services;

public class TenantManagementService : ITenantService
{
    // Identifier — это поддомен тенанта: допустимая DNS-метка в нижнем регистре
    private static readonly Regex IdentifierPattern = new("^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])$", RegexOptions.Compiled);

    // "management" используется статической стратегией самого Tenant-сервиса
    private static readonly HashSet<string> ReservedIdentifiers = new() { "management", "www", "api" };

    private readonly ITenantRepository _tenantRepository;
    private readonly IEventOutbox _eventOutbox;
    private readonly ILogger<TenantManagementService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public TenantManagementService(ITenantRepository tenantRepository,
        IEventOutbox eventOutbox,
        ILogger<TenantManagementService> logger,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _eventOutbox = eventOutbox;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantReadDto>> AddAsync(TenantCreateDto tenant, TenantCaller caller)
    {
        if (tenant == null)
            return Result.Failure<TenantReadDto>(TenantErrors.TenantIsNull);
        if (string.IsNullOrWhiteSpace(tenant.Name))
            return Result.Failure<TenantReadDto>(ValidationErrors.RequiredField);
        if (!IsValidIdentifier(tenant.Identifier))
            return Result.Failure<TenantReadDto>(TenantErrors.InvalidIdentifier);

        // Тенант, созданный владельцем, принадлежит ему; у владельца может быть только один тенант
        int? ownerUserId = null;
        if (!caller.IsGodMode)
        {
            if (caller.UserId == null)
                return Result.Failure<TenantReadDto>(AuthErrors.InvalidCredentials);
            if (await _tenantRepository.GetByOwnerAsync(caller.UserId.Value) != null)
                return Result.Failure<TenantReadDto>(TenantErrors.OwnerAlreadyHasTenant);
            ownerUserId = caller.UserId;
        }

        if (await _tenantRepository.GetByIdentifierAsync(tenant.Identifier) != null)
            return Result.Failure<TenantReadDto>(TenantErrors.TenantAlreadyExists);

        TenantInfoEntity newTenant = new TenantInfoEntity()
        {
            Id = Guid.NewGuid().ToString(),
            Identifier = tenant.Identifier,
            Name = tenant.Name.Trim(),
            IsActive = true,
            OwnerUserId = ownerUserId
        };
        var tenantEvent = new TenantRegisteredEvent();
        FillEvent(tenantEvent, newTenant);
        if (!await SaveWithEventAsync(() => _tenantRepository.AddAsync(newTenant), tenantEvent))
            return Result.Failure<TenantReadDto>(SystemErrors.DatabaseError);
        _logger.LogInformation($"{newTenant.Id} зарегистрирован!");
        return Result.Success(TenantReadDto.FromTenant(newTenant));
    }

    public async Task<Result<TenantReadDto>> UpdateAsync(TenantUpdateDto tenant, TenantCaller caller)
    {
        if (tenant == null)
            return Result.Failure<TenantReadDto>(TenantErrors.TenantIsNull);
        if (string.IsNullOrWhiteSpace(tenant.Name))
            return Result.Failure<TenantReadDto>(ValidationErrors.RequiredField);
        var tempTenant = await _tenantRepository.GetByIdAsync(tenant.Id);
        if (tempTenant == null)
            return Result.Failure<TenantReadDto>(TenantErrors.TenantNotFound);
        if (!CanManage(tempTenant, caller))
            return Result.Failure<TenantReadDto>(AuthErrors.Forbidden);
        if (tenant.IsActive != null && tenant.IsActive != tempTenant.IsActive)
        {
            if (!caller.IsGodMode)
                return Result.Failure<TenantReadDto>(AuthErrors.Forbidden);
            tempTenant.IsActive = tenant.IsActive.Value;
        }
        tempTenant.Name = tenant.Name.Trim();

        var tenantEvent = new TenantUpdatedEvent();
        FillEvent(tenantEvent, tempTenant);
        if (!await SaveWithEventAsync(() => _tenantRepository.UpdateAsync(tempTenant), tenantEvent))
            return Result.Failure<TenantReadDto>(SystemErrors.DatabaseError);
        _logger.LogInformation($"{tempTenant.Id} данные обновлены!");
        return Result.Success(TenantReadDto.FromTenant(tempTenant));
    }

    public async Task<Result> DeleteAsync(string id, TenantCaller caller)
    {
        if (id == null)
            return Result.Failure(TenantErrors.TenantIsNull);
        var tempTenant = await _tenantRepository.GetByIdAsync(id);
        if (tempTenant == null)
            return Result.Failure(TenantErrors.TenantNotFound);
        if (!CanManage(tempTenant, caller))
            return Result.Failure(AuthErrors.Forbidden);
        var tenantEvent = new TenantDeletedEvent();
        FillEvent(tenantEvent, tempTenant);
        if (!await SaveWithEventAsync(() => _tenantRepository.DeleteAsync(tempTenant), tenantEvent))
            return Result.Failure(SystemErrors.DatabaseError);
        _logger.LogInformation($"{tempTenant.Id} тенант удален!");
        return Result.Success();
    }

    public async Task<Result<TenantReadDto>> GetByIdAsync(string tenantId, TenantCaller caller)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result.Failure<TenantReadDto>(TenantErrors.TenantNotFound);
            if (!CanManage(tenant, caller))
                return Result.Failure<TenantReadDto>(AuthErrors.Forbidden);
            return Result.Success(TenantReadDto.FromTenant(tenant));
        }
        catch (Exception e)
        {
            _logger.LogError($"Ошибка БД при получении тенанта по Id {tenantId}\n{e}");
            return Result.Failure<TenantReadDto>(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<TenantReadDto>> GetMyTenantAsync(TenantCaller caller)
    {
        if (caller.UserId == null)
            return Result.Failure<TenantReadDto>(AuthErrors.InvalidCredentials);
        try
        {
            var tenant = await _tenantRepository.GetByOwnerAsync(caller.UserId.Value);
            if (tenant == null)
                return Result.Failure<TenantReadDto>(TenantErrors.TenantNotFound);
            return Result.Success(TenantReadDto.FromTenant(tenant));
        }
        catch (Exception e)
        {
            _logger.LogError($"Ошибка БД при получении тенанта владельца {caller.UserId}\n{e}");
            return Result.Failure<TenantReadDto>(SystemErrors.DatabaseError);
        }
    }

    // Публичный каталог для портала: только активные тенанты и только название с поддоменом
    public async Task<Result<List<ConnectedTenantDto>>> GetConnectedTenantsAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync() ?? Enumerable.Empty<TenantInfoEntity>();
            var list = tenants
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(t => new ConnectedTenantDto(t.Name ?? t.Identifier, t.Identifier))
                .ToList();
            return Result.Success(list);
        }
        catch (Exception e)
        {
            _logger.LogError($"Ошибка БД при получении каталога тенантов\n{e}");
            return Result.Failure<List<ConnectedTenantDto>>(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<TenantReadDto>> GetByNameAsync(string tenantName)
    {
        try
        {
            var tenant = await _tenantRepository.GetByNameAsync(tenantName);
            if (tenant == null)
                return Result.Failure<TenantReadDto>(TenantErrors.TenantNotFound);
            return Result.Success(TenantReadDto.FromTenant(tenant));
        }
        catch (Exception e)
        {
            _logger.LogError($"Ошибка БД при получении тенанта по Name {tenantName}\n{e}");
            return Result.Failure<TenantReadDto>(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<List<TenantReadDto>>> GetAllAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync();
            List<TenantReadDto> list = new List<TenantReadDto>();
            foreach (var tenant in tenants ?? Enumerable.Empty<TenantInfoEntity>())
            {
                list.Add(TenantReadDto.FromTenant(tenant));
            }
            return Result.Success(list);
        }
        catch (Exception e)
        {
            _logger.LogError($"Ошибка БД при получении всех тенантов\n{e}");
            return Result.Failure<List<TenantReadDto>>(SystemErrors.DatabaseError);
        }
    }

    // GodMode — любой тенант, TenantOwner — только свой
    private static bool CanManage(TenantInfoEntity tenant, TenantCaller caller) =>
        caller.IsGodMode
        || (caller.UserId != null && tenant.OwnerUserId == caller.UserId);

    private static bool IsValidIdentifier(string? identifier) =>
        identifier != null
        && IdentifierPattern.IsMatch(identifier)
        && !ReservedIdentifiers.Contains(identifier);

    private static void FillEvent(TenantEvent tenantEvent, TenantInfoEntity tenant)
    {
        tenantEvent.Id = tenant.Id;
        tenantEvent.Identifier = tenant.Identifier;
        tenantEvent.Name = tenant.Name;
        tenantEvent.IsActive = tenant.IsActive;
        tenantEvent.OwnerUserId = tenant.OwnerUserId;
    }

    // Изменение тенанта и событие для других сервисов сохраняются в одной транзакции (Transactional Outbox):
    // либо записано и то и другое, либо ничего. Публикацию в RabbitMQ выполняет фоновый OutboxPublisher
    private async Task<bool> SaveWithEventAsync(Func<Task> changeTenant, TenantEvent tenantEvent)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await changeTenant();
            _eventOutbox.Enqueue(tenantEvent);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollBackTransactionAsync();
            _logger.LogError($"Ошибка БД: {tenantEvent.EventType} {tenantEvent.Id} не сохранен, изменения отменены\n{e}");
            return false;
        }
    }
}
