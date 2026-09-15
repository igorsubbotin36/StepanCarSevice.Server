using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Models.DTOs;

namespace StepanCarService.TenantService.Application.Interfaces;

public interface ITenantService
{
    Task<Result<TenantReadDto>> AddAsync(TenantCreateDto tenant);
    Task<Result<TenantReadDto>> UpdateAsync(TenantUpdateDto tenant);
    Task<Result> DeleteAsync(string id);
    Task<Result<TenantReadDto>> GetByIdAsync(string tenantId);
    Task<Result<TenantReadDto>> GetByNameAsync(string tenantName);
    Task<Result<List<TenantReadDto>>> GetAllAsync();
}
