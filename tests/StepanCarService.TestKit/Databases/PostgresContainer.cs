using Npgsql;
using Testcontainers.PostgreSql;

namespace StepanCarService.TestKit.Databases;

// Один контейнер PostgreSQL на процесс тестового проекта. Запускается при первом обращении,
// поэтому прогон только unit-тестов Docker не требует. Контейнер удаляет Testcontainers (Ryuk) после завершения процесса
public static class PostgresContainer
{
    public const string Image = "postgres:18";

    private static readonly Lazy<Task<PostgreSqlContainer>> Container = new(StartAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public static async Task<string> GetAdminConnectionStringAsync()
    {
        var container = await Container.Value;
        return container.GetConnectionString();
    }

    // Отдельная БД с уникальным именем: тестовые классы не мешают друг другу
    public static async Task<string> CreateDatabaseAsync(string prefix)
    {
        var admin = await GetAdminConnectionStringAsync();
        var name = $"{prefix}_{Guid.NewGuid():N}".ToLowerInvariant();
        await using (var connection = new NpgsqlConnection(admin))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }
        return new NpgsqlConnectionStringBuilder(admin) { Database = name }.ConnectionString;
    }

    public static async Task DropDatabaseAsync(string connectionString)
    {
        var name = new NpgsqlConnectionStringBuilder(connectionString).Database;
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(await GetAdminConnectionStringAsync());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<PostgreSqlContainer> StartAsync()
    {
        var container = new PostgreSqlBuilder(Image)
            // Данные тестов не нужно переживать сбой: без fsync БД заметно быстрее
            .WithCommand("-c", "fsync=off", "-c", "synchronous_commit=off", "-c", "full_page_writes=off")
            .Build();
        await container.StartAsync();
        return container;
    }
}
