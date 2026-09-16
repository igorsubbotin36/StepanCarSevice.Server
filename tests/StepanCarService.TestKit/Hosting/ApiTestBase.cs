using StepanCarService.Common.Infastructure.DbContexts;

namespace StepanCarService.TestKit.Hosting;

// Базовый класс API-тестов: сервис поднимается один раз на класс, перед каждым тестом данные очищаются
public abstract class ApiTestBase<TFactory, TEntryPoint, TDbContext> : IClassFixture<TFactory>, IAsyncLifetime
    where TFactory : ServiceFactory<TEntryPoint, TDbContext>
    where TEntryPoint : class
    where TDbContext : TenantBaseDbContext
{
    protected ApiTestBase(TFactory factory)
    {
        Factory = factory;
    }

    protected TFactory Factory { get; }

    public virtual async ValueTask InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
