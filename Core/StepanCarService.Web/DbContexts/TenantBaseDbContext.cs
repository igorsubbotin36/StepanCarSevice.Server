using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Core.Entities;

namespace StepanCarService.Web.DbContexts
{
    public class TenantBaseDbContext : EFCoreStoreDbContext<TenantInfoEntity>
    {
        public TenantBaseDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TenantInfoEntity> Tenants { get; set; }
    }
}
