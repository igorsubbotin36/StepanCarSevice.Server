using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Jwt;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.TestKit.Hosting;

// Сервис целиком в процессе теста (WebApplicationFactory) на своей БД в контейнере PostgreSQL.
// Используется как IClassFixture: один запуск сервиса на тестовый класс, миграции применяются при старте, как в проде.
//  - Настройки передаются через UseSetting: они видны уже в Program.cs до builder.Build()
//    и перекрывают user-secrets машины разработчика.
//  - Фоновые службы сервиса (consumer RabbitMQ, outbox) по умолчанию выключены; включаются в messaging- и E2E-тестах.
public abstract class ServiceFactory<TEntryPoint, TDbContext> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : TenantBaseDbContext
{
    private ServiceDatabase? _database;

    // Префикс имени тестовой БД
    protected abstract string DatabasePrefix { get; }

    // Таблицы, которые не очищаются между тестами (сид-справочники)
    protected virtual IEnumerable<string> TablesToKeep => [];

    // Запускать ли фоновые службы сервиса (TenantEventsConsumer, OutboxPublisher)
    protected virtual bool RunBackgroundServices => false;

    public string ConnectionString => Database.ConnectionString;

    private ServiceDatabase Database =>
        _database ?? throw new InvalidOperationException("Фабрика не инициализирована: вызовите InitializeAsync (xUnit делает это для IClassFixture)");

    public virtual async ValueTask InitializeAsync()
    {
        _database = new ServiceDatabase(DatabasePrefix, TablesToKeep);
        await _database.InitializeAsync();
        // Обращение к Server запускает сервис: Program.cs выполняется целиком, включая миграции
        _ = Server;
    }

    // Базовые настройки теста; наследник может дополнить или переопределить
    protected virtual IDictionary<string, string?> GetSettings() => new Dictionary<string, string?>
    {
        ["ConnectionStrings:PostgreSQL"] = ConnectionString,
        ["Jwt:Key"] = TestTokenFactory.Key,
        ["Jwt:Issuer"] = TestTokenFactory.Issuer,
        ["Jwt:Audience"] = TestTokenFactory.Audience,
        ["Jwt:LifetimeMinutes"] = "30",
        // Лимиты запросов не мешают сценариям с множеством входов; тесты лимитов задают свои значения
        ["RateLimiting:Auth:PermitLimit"] = "100000",
        ["RateLimiting:Public:PermitLimit"] = "100000",
        // Без явного vhost сервис не должен попасть в RabbitMQ машины разработчика
        ["RabbitMQ:HostName"] = "127.0.0.1",
        ["RabbitMQ:Port"] = "1",
        ["RabbitMQ:VirtualHost"] = "not-configured-in-test"
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        foreach (var (key, value) in GetSettings())
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            if (!RunBackgroundServices)
                RemoveServiceBackgroundServices(services);
            ConfigureTestServices(services);
        });
    }

    // Точка расширения для подмены зависимостей в конкретном наборе тестов
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    // Отдельный запуск сервиса поверх настроек фабрики, но с другим окружением и частью настроек (тесты старта).
    // Возвращает исключение, с которым сервис не запустился, или null, если запуск успешен
    public Exception? TryStart(string environment, IDictionary<string, string?> settings)
    {
        using var factory = WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            foreach (var (key, value) in settings)
                builder.UseSetting(key, value);
        });
        try
        {
            _ = factory.Server;
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    public HttpClient CreateClientFor(string? tenantIdentifier = null) =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = tenantIdentifier == null ? TenantHost.PortalAddress : TenantHost.AddressFor(tenantIdentifier),
            AllowAutoRedirect = false
        });

    // Копия тенанта в БД сервиса (в проде её наполняют события из Tenant-сервиса)
    public async Task<TenantInfoEntity> SeedTenantAsync(TenantInfoEntity tenant)
    {
        await WithDbContextAsync(async db =>
        {
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        });
        return tenant;
    }

    public async Task WithDbContextAsync(Func<TDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<TDbContext>());
    }

    public async Task<T> WithDbContextAsync<T>(Func<TDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TDbContext>());
    }

    // Очистка данных между тестами класса (схема, история миграций и справочники остаются)
    public Task ResetDatabaseAsync() => Database.ResetAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_database != null)
            await _database.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    // Фоновые службы самого сервиса; служебные службы ASP.NET Core (веб-сервер) не трогаем
    private static void RemoveServiceBackgroundServices(IServiceCollection services)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService)
                && d.ImplementationType?.Assembly.GetName().Name?.StartsWith("StepanCar", StringComparison.Ordinal) == true)
            .ToList();
        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }

    private sealed class ServiceDatabase(string prefix, IEnumerable<string> tablesToKeep) : PostgresDatabase
    {
        protected override string Prefix => prefix;
        protected override IEnumerable<string> TablesToKeep => tablesToKeep;
    }
}
