using Microsoft.Extensions.DependencyInjection;
using StepanCarSevice.DetailService.API;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TestKit.Hosting;

namespace StepanCarService.DetailService.Tests.Startup;

// Запуск Detail: общие регистрации и отказ стартовать с опасной конфигурацией.
// Контроллеры и сервисы Detail проверяются после завершения разработки сервиса
[Trait(TestCategories.Name, TestCategories.Startup)]
[Collection(TestCollections.Database)]
public class DetailStartupConfigurationTests(DetailServiceFactory factory)
    : ApiTestBase<DetailServiceFactory, Program, DetailDbContext>(factory)
{
    [Theory]
    [InlineData(typeof(IUnitOfWork))]
    [InlineData(typeof(IErrorMapper))]
    [InlineData(typeof(ITenantRepository))]
    [InlineData(typeof(ITenantEventHandler))]
    public async Task SharedServices_Resolve(Type serviceType)
    {
        await using var scope = Factory.Services.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService(serviceType).ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("*")]
    public void Production_WithoutExplicitAllowedHosts_DoesNotStart(string? allowedHosts)
    {
        Factory.TryStart("Production", new Dictionary<string, string?> { ["AllowedHosts"] = allowedHosts })
            .ShouldNotBeNull().ToString().ShouldContain("AllowedHosts");
    }

    [Theory]
    [InlineData("", "JWT Key is not configured")]
    [InlineData("your_private_key", "JWT Key is too short")]
    public void Production_WithoutValidJwtKey_DoesNotStart(string key, string expectedMessage)
    {
        Factory.TryStart("Production", new Dictionary<string, string?> { ["AllowedHosts"] = "example.com", ["Jwt:Key"] = key })
            .ShouldNotBeNull().ToString().ShouldContain(expectedMessage);
    }

    [Fact]
    public void Production_WithValidConfiguration_Starts()
    {
        Factory.TryStart("Production", new Dictionary<string, string?> { ["AllowedHosts"] = "example.com" }).ShouldBeNull();
    }
}
