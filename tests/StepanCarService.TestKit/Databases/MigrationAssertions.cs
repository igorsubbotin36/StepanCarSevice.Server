using Microsoft.EntityFrameworkCore;

namespace StepanCarService.TestKit.Databases;

// Общие проверки миграций DbContext сервиса
public static class MigrationAssertions
{
    // Все миграции применяются к только что созданной пустой БД
    public static async Task ShouldApplyToEmptyDatabaseAsync<TDbContext>(string prefix, Func<string, TDbContext> createContext)
        where TDbContext : DbContext
    {
        var connectionString = await PostgresContainer.CreateDatabaseAsync(prefix);
        try
        {
            await using var db = createContext(connectionString);
            await db.Database.MigrateAsync();

            var applied = await db.Database.GetAppliedMigrationsAsync();
            applied.ShouldNotBeEmpty();
            applied.ShouldBe(db.Database.GetMigrations());
            (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        }
        finally
        {
            await PostgresContainer.DropDatabaseAsync(connectionString);
        }
    }

    // Модель совпадает с последней миграцией: изменение сущностей без новой миграции не попадёт в прод
    public static void ShouldHaveNoPendingModelChanges(this DbContext db)
    {
        db.Database.HasPendingModelChanges().ShouldBeFalse(
            $"модель {db.GetType().Name} изменена без миграции: dotnet ef migrations add <Name>");
    }
}
