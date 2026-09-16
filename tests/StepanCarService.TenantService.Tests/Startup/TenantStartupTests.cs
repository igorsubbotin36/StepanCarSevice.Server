using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TenantService.API.Controllers;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Hosting;

namespace StepanCarService.TenantService.Tests.Startup;

// Запуск Tenant-сервиса: регистрации зависимостей и отказ стартовать с опасной конфигурацией
[Trait(TestCategories.Name, TestCategories.Startup)]
[Collection(TestCollections.Database)]
public class TenantStartupTests(TenantServiceFactory factory)
    : ApiTestBase<TenantServiceFactory, Program, TenantServiceDbContext>(factory)
{
    [Fact]
    public async Task Controllers_ResolveAllDependencies()
    {
        var controllers = typeof(TenantController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();
        await using var scope = Factory.Services.CreateAsyncScope();

        controllers.ShouldNotBeEmpty();
        foreach (var controller in controllers)
            Should.NotThrow(() => ActivatorUtilities.CreateInstance(scope.ServiceProvider, controller), controller.Name);
    }

    [Theory]
    [InlineData(typeof(IUnitOfWork))]
    [InlineData(typeof(IErrorMapper))]
    [InlineData(typeof(ITenantRepository))]
    [InlineData(typeof(ITenantService))]
    [InlineData(typeof(IEventOutbox))]
    [InlineData(typeof(IMessageBus))]
    public async Task Services_Resolve(Type serviceType)
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
    [InlineData("short-key", "JWT Key is too short")]
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
