using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Models.DTOs;

namespace StepanCarService.TenantService.Application.Interfaces;

public interface ITenantService
{
    Task<Result> AddAsync(TenantDto tenant);
    Task<Result> UpdateAsync(TenantDto tenant);
    Task<Result> DeleteAsync(string id);
    Task<Result<TenantDto>> GetByIdAsync(string tenantId);
    Task<Result<TenantDto>> GetByNameAsync(string tenantName);
    Task<Result<List<TenantDto>>> GetAllAsync();
}