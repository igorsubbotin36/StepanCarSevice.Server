using Microsoft.EntityFrameworkCore;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.VisitService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class VisitMigrationsTests(VisitDatabase database) : DatabaseTestBase<VisitDatabase>(database)
{
    private static VisitDBContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<VisitDBContext>().UseNpgsql(connectionString).Options);

    // Миграции применились к пустой БД, данных тенантов вне тенанта не видно
    [Fact]
    public async Task Migrations_AppliedToEmptyDatabase()
    {
        await using var db = Database.CreateContext();

        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await db.Visits.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public Task Migrations_ApplyToEmptyDatabase() =>
        MigrationAssertions.ShouldApplyToEmptyDatabaseAsync("mig_visit", CreateContext);

    [Fact]
    public void Model_MatchesLastMigration()
    {
        using var db = CreateContext("Host=localhost");

        db.ShouldHaveNoPendingModelChanges();
    }
}
