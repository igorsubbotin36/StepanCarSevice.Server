using StepanCarService.TenantService.Domain.Entities;

namespace StepanCarService.TenantService.Domain.Repositories;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task DeleteAsync(Tenant tenant);
    Task<Tenant?> GetByIdAsync(string tenantId);
    Task<Tenant?> GetByNameAsync(string tenantName);
    Task<IEnumerable<Tenant>?> GetAllAsync();
}