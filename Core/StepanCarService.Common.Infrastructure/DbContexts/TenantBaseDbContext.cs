using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;

namespace StepanCarService.Common.Infastructure.DbContexts
{
    public class TenantBaseDbContext : EFCoreStoreDbContext<TenantInfoEntity>
    {
        public TenantBaseDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TenantInfoEntity> Tenants { get; set; }
    }
}
