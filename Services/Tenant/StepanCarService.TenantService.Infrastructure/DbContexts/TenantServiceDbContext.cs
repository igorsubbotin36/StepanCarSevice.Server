using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Core.Entities;

namespace StepanCarService.TenantService.Infrastructure.DbContexts;

public class TenantServiceDbContext : EFCoreStoreDbContext<TenantInfoEntity>
{
    public DbSet<TenantInfoEntity> Tenants { get; set; }

    public TenantServiceDbContext(DbContextOptions<TenantServiceDbContext> options) : base(options) { }

    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            optionsBuilder.UseNpgsql(connectionString);
        }
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantServiceDbContext).Assembly);
    }
}