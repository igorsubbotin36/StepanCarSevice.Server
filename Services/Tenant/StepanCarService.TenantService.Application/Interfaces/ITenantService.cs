using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Models.DTOs;

namespace StepanCarService.TenantService.Application.Interfaces;

public interface ITenantService
{
    Task<Result<TenantReadDto>> AddAsync(TenantCreateDto tenant, TenantCaller caller);
    Task<Result<TenantReadDto>> UpdateAsync(TenantUpdateDto tenant, TenantCaller caller);
    Task<Result> DeleteAsync(string id, TenantCaller caller);
    Task<Result<TenantReadDto>> GetByIdAsync(string tenantId, TenantCaller caller);
    Task<Result<TenantReadDto>> GetMyTenantAsync(TenantCaller caller);
    Task<Result<List<ConnectedTenantDto>>> GetConnectedTenantsAsync();
    Task<Result<TenantReadDto>> GetByNameAsync(string tenantName);
    Task<Result<List<TenantReadDto>>> GetAllAsync();
}
