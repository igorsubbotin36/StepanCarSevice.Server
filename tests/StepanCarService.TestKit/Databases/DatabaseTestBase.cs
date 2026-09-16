namespace StepanCarService.TestKit.Databases;

// Базовый класс интеграционных тестов с БД: своя БД на класс, перед каждым тестом данные очищаются
public abstract class DatabaseTestBase<TDatabase> : IClassFixture<TDatabase>, IAsyncLifetime
    where TDatabase : PostgresDatabase
{
    protected DatabaseTestBase(TDatabase database)
    {
        Database = database;
    }

    protected TDatabase Database { get; }

    public virtual async ValueTask InitializeAsync()
    {
        await Database.ResetAsync();
    }

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
