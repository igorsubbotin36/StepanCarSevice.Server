using System.Net;
using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.TenantService.Tests.Api;

[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class TenantControllerTests(TenantServiceFactory factory)
    : ApiTestBase<TenantServiceFactory, Program, TenantServiceDbContext>(factory)
{
    [Fact]
    public async Task GetAllTenants_WithoutToken_Returns401()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("api/Tenant/getAllTenants");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Токен из TestTokenFactory принимается сервисом
    [Fact]
    public async Task GetAllTenants_GodModeToken_Returns200()
    {
        using var client = Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.GodMode));

        var response = await client.GetAsync("api/Tenant/getAllTenants");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Публичный каталог без токена
    [Fact]
    public async Task GetConnectedTenants_Anonymous_ReturnsEmptyList()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("api/Tenant/getConnectedTenants");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldBe("[]");
    }
}
