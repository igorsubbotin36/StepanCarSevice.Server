using Npgsql;
using Respawn;
using Respawn.Graph;

namespace StepanCarService.TestKit.Databases;

// Фикстура «своя БД на тестовый класс»: IClassFixture<PostgresDatabase>.
// Между тестами класса данные очищаются через ResetAsync (Respawn), схема и справочники остаются
public class PostgresDatabase : IAsyncLifetime
{
    private Respawner? _respawner;

    // Префикс имени БД (в имени видно, какой набор тестов её создал)
    protected virtual string Prefix => "test";

    // Таблицы, которые не очищаются между тестами: история миграций и сид-справочники
    protected virtual IEnumerable<string> TablesToKeep => [];

    public string ConnectionString { get; private set; } = string.Empty;

    public virtual async ValueTask InitializeAsync()
    {
        ConnectionString = await PostgresContainer.CreateDatabaseAsync(Prefix);
    }

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        // Respawner строится по фактической схеме, поэтому создаётся после миграций — при первой очистке
        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = TablesToKeep.Append("__EFMigrationsHistory").Select(t => new Table(t)).ToArray(),
            WithReseed = true
        });
        await _respawner.ResetAsync(connection);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrEmpty(ConnectionString))
            await PostgresContainer.DropDatabaseAsync(ConnectionString);
        GC.SuppressFinalize(this);
    }
}
