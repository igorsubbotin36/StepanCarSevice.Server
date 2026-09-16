using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using StepanCarSevice.AuthService.Infrastructure.DBContexts.Inits;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.AuthService.Tests;

// Auth целиком (API-тесты): миграции и сид ролей выполняет сам сервис при старте
public class AuthServiceFactory : ServiceFactory<Program, AuthDbContext>
{
    protected override string DatabasePrefix => "auth";
    protected override IEnumerable<string> TablesToKeep => ["Roles"];
}

// БД Auth без HTTP (интеграционные тесты репозиториев и схемы)
public class AuthDatabase : ServiceDatabase<AuthDbContext>
{
    protected override string Prefix => "auth";
    protected override IEnumerable<string> TablesToKeep => ["Roles"];

    // AuthDbContext не зависит от тенанта запроса
    public override AuthDbContext CreateContext(FakeTenantAccessor tenantAccessor) => new(CreateOptions());

    protected override Task SeedAsync(AuthDbContext db)
    {
        DbInitializer.Init(db);
        return Task.CompletedTask;
    }
}
