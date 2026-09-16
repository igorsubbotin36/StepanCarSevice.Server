using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;

namespace StepanCarService.Common.Infastructure.Stores
{
    internal sealed class CaseInsensitiveTenantStore<TContext> : IMultiTenantStore<TenantInfoEntity>
        where TContext : TenantBaseDbContext
    {
        private readonly TContext _dbContext;

        public CaseInsensitiveTenantStore(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TenantInfoEntity?> TryGetByIdentifierAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;
            return await _dbContext.Tenants.SingleOrDefaultAsync(t => t.Identifier != null && t.Identifier.ToLower() == identifier.ToLower());
        }

        public async Task<TenantInfoEntity?> TryGetAsync(string id) =>
            await _dbContext.Tenants.SingleOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<TenantInfoEntity>> GetAllAsync() =>
            await _dbContext.Tenants.ToListAsync();

        public async Task<IEnumerable<TenantInfoEntity>> GetAllAsync(int skip, int take) =>
            await _dbContext.Tenants.OrderBy(t => t.Id).Skip(skip).Take(take).ToListAsync();

        public async Task<bool> TryAddAsync(TenantInfoEntity tenantInfo)
        {
            await _dbContext.Tenants.AddAsync(tenantInfo);
            return await SaveAsync();
        }

        public async Task<bool> TryUpdateAsync(TenantInfoEntity tenantInfo)
        {
            _dbContext.Tenants.Update(tenantInfo);
            return await SaveAsync();
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var entity = await TryGetByIdentifierAsync(identifier);
            if (entity == null)
                return false;
            _dbContext.Tenants.Remove(entity);
            return await SaveAsync();
        }

        private async Task<bool> SaveAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
