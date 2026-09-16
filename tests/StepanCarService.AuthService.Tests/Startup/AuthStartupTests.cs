using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TestKit.Hosting;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarService.AuthService.Tests.Startup;

// Запуск Auth: регистрации зависимостей и отказ стартовать с опасной конфигурацией
[Trait(TestCategories.Name, TestCategories.Startup)]
[Collection(TestCollections.Database)]
public class AuthStartupTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    // Сервис собран в Development, где ValidateOnBuild и ValidateScopes включены по умолчанию
    // Каждый контроллер создаётся со всеми зависимостями
    [Fact]
    public async Task Controllers_ResolveAllDependencies()
    {
        var controllers = typeof(StepanCarSevice.AuthService.Controllers.AuthController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();
        await using var scope = Factory.Services.CreateAsyncScope();

        controllers.Count.ShouldBe(2);
        foreach (var controller in controllers)
            Should.NotThrow(() => ActivatorUtilities.CreateInstance(scope.ServiceProvider, controller), controller.Name);
    }

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
    [InlineData("auth.example.com;*")]
    public void Production_WithoutExplicitAllowedHosts_DoesNotStart(string? allowedHosts)
    {
        var exception = Factory.TryStart("Production", new Dictionary<string, string?> { ["AllowedHosts"] = allowedHosts });

        exception.ShouldNotBeNull().ToString().ShouldContain("AllowedHosts");
    }

    // Без ключа в user-secrets/переменных окружения остаётся заглушка из appsettings.json
    [Theory]
    [InlineData("", "JWT Key is not configured")]
    [InlineData("your_private_key", "JWT Key is too short")]
    public void Production_WithoutValidJwtKey_DoesNotStart(string key, string expectedMessage)
    {
        var exception = Factory.TryStart("Production", new Dictionary<string, string?>
        {
            ["AllowedHosts"] = "auth.example.com",
            ["Jwt:Key"] = key
        });

        exception.ShouldNotBeNull().ToString().ShouldContain(expectedMessage);
    }

    // Корректная прод-конфигурация стартует — отказы выше вызваны именно проверяемыми настройками
    [Fact]
    public void Production_WithValidConfiguration_Starts()
    {
        Factory.TryStart("Production", new Dictionary<string, string?> { ["AllowedHosts"] = "auth.example.com;*.auth.example.com" })
            .ShouldBeNull();
    }
}
