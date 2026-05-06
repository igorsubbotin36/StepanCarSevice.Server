using Microsoft.EntityFrameworkCore;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarSevice.TenantService.Core.Entities;
using StepanCarSevice.TenantService.Core.Repositories;

namespace StepanCarService.TenantService.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly TenantServiceDbContext _tenantServiceDbContext;

    public TenantRepository(TenantServiceDbContext context)
    {
        _tenantServiceDbContext = context;
    }
    public async Task AddAsync(TenantInfoEntity tenant)
    {
        await _tenantServiceDbContext.AddAsync(tenant);
        await _tenantServiceDbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(TenantInfoEntity tenant)
    {
        _tenantServiceDbContext.Tenants.Update(tenant);
        await _tenantServiceDbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(TenantInfoEntity tenant)
    {
        _tenantServiceDbContext.Tenants.Remove(tenant);
        await _tenantServiceDbContext.SaveChangesAsync();
    }

    public async Task<TenantInfoEntity?> GetByIdAsync(string tenantId)
    {
        var entity = await _tenantServiceDbContext.Tenants.FindAsync(tenantId);
        return entity == null ? null : entity;
    }

    public async Task<TenantInfoEntity?> GetByNameAsync(string tenantName)
    {
        var entity = await _tenantServiceDbContext.Tenants.SingleOrDefaultAsync(t => t.Name == tenantName);
        return entity == null ? null : entity;
    }

    public async Task<IEnumerable<TenantInfoEntity>?> GetAllAsync()
    {
        var entities = await _tenantServiceDbContext.Tenants.AsNoTracking().ToListAsync();
        return entities;
    }
}