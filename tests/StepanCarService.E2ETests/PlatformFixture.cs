using StepanCarSevice.AuthService.Controllers;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TenantService.API.Controllers;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Messaging;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;

namespace StepanCarService.E2ETests;

// Вся платформа в процессе теста: 4 сервиса, у каждого своя БД, общий vhost RabbitMQ.
// Фоновые службы включены: события тенантов идут по настоящему пути outbox → exchange → consumer'ы.
// Классы Program Auth и Tenant оба лежат в глобальном пространстве имён, поэтому сервис указывается типом контроллера из его сборки
public sealed class PlatformFixture : IAsyncLifetime
{
    public const string ExchangeName = "tenant.events.exchange";

    private readonly RabbitMqVirtualHost _virtualHost = new();

    public PlatformService<AuthController, AuthDbContext> Auth { get; private set; } = null!;
    public PlatformService<TenantController, TenantServiceDbContext> Tenant { get; private set; } = null!;
    public PlatformService<StepanCarSevice.DetailService.API.Program, DetailDbContext> Detail { get; private set; } = null!;
    public PlatformService<StepanCarSevice.VisitService.API.Program, VisitDBContext> Visit { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _virtualHost.InitializeAsync();
        // Получатели стартуют первыми: их очереди привязываются к exchange до первых событий
        Auth = new("e2e_auth", _virtualHost.ToSettings(ExchangeName, "auth.tenant.events"));
        Detail = new("e2e_detail", _virtualHost.ToSettings(ExchangeName, "detail.tenant.events"));
        Visit = new("e2e_visit", _virtualHost.ToSettings(ExchangeName, "visit.tenant.events"));
        Tenant = new("e2e_tenant", _virtualHost.ToSettings(ExchangeName)
            .Append(new("Outbox:PollIntervalMilliseconds", "100")));
        await Task.WhenAll(
            Auth.InitializeAsync().AsTask(),
            Detail.InitializeAsync().AsTask(),
            Visit.InitializeAsync().AsTask());
        await Tenant.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var service in new IAsyncDisposable?[] { Tenant, Auth, Detail, Visit })
        {
            if (service != null)
                await service.DisposeAsync();
        }
        await _virtualHost.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PlatformCollection : ICollectionFixture<PlatformFixture>
{
    public const string Name = "Platform";
}

public sealed class PlatformService<TEntryPoint, TDbContext>(string databasePrefix, IEnumerable<KeyValuePair<string, string>> settings)
    : ServiceFactory<TEntryPoint, TDbContext>
    where TEntryPoint : class
    where TDbContext : TenantBaseDbContext
{
    protected override string DatabasePrefix => databasePrefix;
    protected override bool RunBackgroundServices => true;

    protected override IDictionary<string, string?> GetSettings()
    {
        var result = base.GetSettings();
        foreach (var (key, value) in settings)
            result[key] = value;
        return result;
    }
}
