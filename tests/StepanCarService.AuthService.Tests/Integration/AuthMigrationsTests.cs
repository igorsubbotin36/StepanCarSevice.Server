using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Databases;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarService.AuthService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class AuthMigrationsTests
{
    private static AuthDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(connectionString).Options);

    [Fact]
    public Task Migrations_ApplyToEmptyDatabase() =>
        MigrationAssertions.ShouldApplyToEmptyDatabaseAsync("mig_auth", CreateContext);

    [Fact]
    public void Model_MatchesLastMigration()
    {
        using var db = CreateContext("Host=localhost");

        db.ShouldHaveNoPendingModelChanges();
    }
}
