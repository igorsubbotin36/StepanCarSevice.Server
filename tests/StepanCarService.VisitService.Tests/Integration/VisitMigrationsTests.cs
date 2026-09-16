using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.VisitService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class VisitMigrationsTests(VisitDatabase database) : DatabaseTestBase<VisitDatabase>(database)
{
    // VS-33 (часть): миграции применились к пустой БД, данных тенантов вне тенанта не видно
    [Fact]
    public async Task Migrations_AppliedToEmptyDatabase()
    {
        await using var db = Database.CreateContext();

        (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await db.Visits.CountAsync()).ShouldBe(0);
    }
}
