using StepanCarSevice.TenantService.Core.Entities;

namespace StepanCarSevice.TenantService.Core.Repositories;

public interface ITenantRepository
{
    Task AddAsync(TenantInfoEntity tenant);
    Task UpdateAsync(TenantInfoEntity tenant);
    Task DeleteAsync(TenantInfoEntity tenant);
    Task<TenantInfoEntity?> GetByIdAsync(string tenantId);
    Task<TenantInfoEntity?> GetByNameAsync(string tenantName);
    Task<IEnumerable<TenantInfoEntity>?> GetAllAsync();
}