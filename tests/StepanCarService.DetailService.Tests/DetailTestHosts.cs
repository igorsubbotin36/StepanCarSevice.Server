using StepanCarSevice.DetailService.API;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarSevice.DetailService.Infrastructure.DBContexts.Inits;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.DetailService.Tests;

internal static class DetailTables
{
    // Справочники из сида: между тестами не очищаются
    public static readonly string[] Dictionaries = ["EngineTypes", "TransmissionTypes", "WheelDriveTypes"];
}

// Detail целиком: миграции и сид справочников выполняет сам сервис при старте
public class DetailServiceFactory : ServiceFactory<Program, DetailDbContext>
{
    protected override string DatabasePrefix => "detail";
    protected override IEnumerable<string> TablesToKeep => DetailTables.Dictionaries;
}

// БД Detail без HTTP; контекст создаётся в нужном тенанте (FakeTenantAccessor)
public class DetailDatabase : ServiceDatabase<DetailDbContext>
{
    protected override string Prefix => "detail";
    protected override IEnumerable<string> TablesToKeep => DetailTables.Dictionaries;

    public override DetailDbContext CreateContext(FakeTenantAccessor tenantAccessor) => new(CreateOptions(), tenantAccessor);

    protected override Task SeedAsync(DetailDbContext db)
    {
        DbInitializer.Init(db);
        return Task.CompletedTask;
    }
}
