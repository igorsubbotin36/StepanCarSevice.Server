using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.TestKit.Databases;

// БД сервиса для интеграционных тестов без HTTP: схема создаётся настоящими миграциями DbContext'а.
// Наследник в тестовом проекте сервиса только создаёт контекст (и при необходимости выполняет сид)
public abstract class ServiceDatabase<TDbContext> : PostgresDatabase
    where TDbContext : TenantBaseDbContext
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await SeedAsync(db);
    }

    // Контекст в тенанте tenant; null — портал, фоновая задача, миграции
    public TDbContext CreateContext(TenantInfoEntity? tenant = null) => CreateContext(new FakeTenantAccessor(tenant));

    public abstract TDbContext CreateContext(FakeTenantAccessor tenantAccessor);

    protected DbContextOptions<TDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TDbContext>().UseNpgsql(ConnectionString).Options;

    protected virtual Task SeedAsync(TDbContext db) => Task.CompletedTask;

    public async Task<TenantInfoEntity> SeedTenantAsync(TenantInfoEntity tenant)
    {
        await using var db = CreateContext();
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    }
}
