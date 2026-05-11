using StepanCarService.Core.Entities;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.Core.Models;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Models.DTOs;

namespace StepanCarService.TenantService.Application.Services;

public class TenantManagementService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    public TenantManagementService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    public async Task<Result> AddAsync(TenantDto tenant)
    {
        if (tenant == null)
            return Result.Failure(TenantErrors.TenantIsNull);
        TenantInfoEntity newTenant = new TenantInfoEntity()
        {
            Id = tenant.Id,
            Name = tenant.Name,
            ConnectionString = tenant.ConnectionString,
            IsActive = tenant.IsActive,
            ApiKey = tenant.ApiKey
        };
        try
        {
            await _tenantRepository.AddAsync(newTenant);
            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result> UpdateAsync(TenantDto tenant)
    {
        if (tenant == null)
            return Result.Failure(TenantErrors.TenantIsNull);
        var tempTenant = await _tenantRepository.GetByIdAsync(tenant.Id);
        if (tempTenant == null)
            return Result.Failure(TenantErrors.TenantNotFound);
        tempTenant.ConnectionString = tenant.ConnectionString;
        tempTenant.IsActive = tenant.IsActive;
        tempTenant.ApiKey = tenant.ApiKey;
        tempTenant.Name = tenant.Name;
        try
        {
            await _tenantRepository.UpdateAsync(tempTenant);
            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result> DeleteAsync(string id)
    {
        if (id == null)
            return Result.Failure(TenantErrors.TenantIsNull);
        var tempTenant = await _tenantRepository.GetByIdAsync(id);
        if (tempTenant == null)
            return Result.Failure(TenantErrors.TenantNotFound);
        try
        {
            await _tenantRepository.DeleteAsync(tempTenant);
            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<TenantDto>> GetByIdAsync(string tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result.Failure<TenantDto>(TenantErrors.TenantNotFound);
            return Result.Success<TenantDto>(TenantDto.FromTenant(tenant));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure<TenantDto>(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<TenantDto>> GetByNameAsync(string tenantName)
    {
        try
        {
            var tenant = await _tenantRepository.GetByNameAsync(tenantName);
            if (tenant == null)
                return Result.Failure<TenantDto>(TenantErrors.TenantNotFound);
            return Result.Success<TenantDto>(TenantDto.FromTenant(tenant));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure<TenantDto>(SystemErrors.DatabaseError);
        }
    }

    public async Task<Result<List<TenantDto>>> GetAllAsync()
    {
        try
        {
            var tenants = await _tenantRepository.GetAllAsync();
            List<TenantDto> list = new List<TenantDto>();
            foreach (var tenant in tenants)
            {
                list.Add(TenantDto.FromTenant(tenant));
            }
            return Result.Success<List<TenantDto>>(list);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Result.Failure<List<TenantDto>>(SystemErrors.DatabaseError);
        }
    }
}