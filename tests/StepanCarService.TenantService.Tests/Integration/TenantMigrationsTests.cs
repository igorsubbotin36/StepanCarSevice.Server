using Microsoft.EntityFrameworkCore;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.TenantService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class TenantMigrationsTests
{
    private static TenantServiceDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<TenantServiceDbContext>().UseNpgsql(connectionString).Options);

    [Fact]
    public Task Migrations_ApplyToEmptyDatabase() =>
        MigrationAssertions.ShouldApplyToEmptyDatabaseAsync("mig_tenant", CreateContext);

    [Fact]
    public void Model_MatchesLastMigration()
    {
        using var db = CreateContext("Host=localhost");

        db.ShouldHaveNoPendingModelChanges();
    }
}
