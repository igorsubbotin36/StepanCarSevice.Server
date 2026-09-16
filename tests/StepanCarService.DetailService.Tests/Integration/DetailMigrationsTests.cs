using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.DetailService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class DetailMigrationsTests
{
    private static DetailDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<DetailDbContext>().UseNpgsql(connectionString).Options);

    [Fact]
    public Task Migrations_ApplyToEmptyDatabase() =>
        MigrationAssertions.ShouldApplyToEmptyDatabaseAsync("mig_detail", CreateContext);

    [Fact]
    public void Model_MatchesLastMigration()
    {
        using var db = CreateContext("Host=localhost");

        db.ShouldHaveNoPendingModelChanges();
    }
}
