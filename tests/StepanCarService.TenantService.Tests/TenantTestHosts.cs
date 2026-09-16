using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.TenantService.Tests;

// Tenant целиком (API-тесты); OutboxPublisher выключен — события остаются в таблице OutboxMessages
public class TenantServiceFactory : ServiceFactory<Program, TenantServiceDbContext>
{
    protected override string DatabasePrefix => "tenant";
}

// БД Tenant без HTTP (outbox, репозитории, схема)
public class TenantDatabase : ServiceDatabase<TenantServiceDbContext>
{
    protected override string Prefix => "tenant";

    public override TenantServiceDbContext CreateContext(FakeTenantAccessor tenantAccessor) => new(CreateOptions());
}
