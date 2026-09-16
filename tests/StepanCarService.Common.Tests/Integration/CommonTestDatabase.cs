using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.Common.Tests.Integration;

// Минимальный контекст на базе TenantBaseDbContext (только таблица тенантов) — для проверки общей инфраструктуры данных
public sealed class CommonTestDbContext(DbContextOptions<CommonTestDbContext> options) : TenantBaseDbContext(options);

public sealed class CommonTestDatabase : PostgresDatabase
{
    protected override string Prefix => "common";

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var db = CreateContext();
        // У тестового контекста нет миграций: схема создаётся по модели
        await db.Database.EnsureCreatedAsync();
    }

    public CommonTestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CommonTestDbContext>().UseNpgsql(ConnectionString).Options);
}
