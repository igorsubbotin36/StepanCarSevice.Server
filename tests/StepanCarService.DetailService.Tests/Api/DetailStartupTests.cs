using System.Net;
using StepanCarSevice.DetailService.API;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;

namespace StepanCarService.DetailService.Tests.Api;

[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class DetailStartupTests(DetailServiceFactory factory)
    : ApiTestBase<DetailServiceFactory, Program, DetailDbContext>(factory)
{
    // Сервис стартует на пустой БД и отвечает на запросы
    [Fact]
    public async Task Service_StartsAndServesOpenApi()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("openapi/v1.json");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // DT-69: копия тенанта (наполняется событиями в проде) уже достаточна, чтобы поддомен тенанта
    // проходил host filtering и обслуживался тем же сервисом
    [Fact]
    public async Task Service_AcceptsRequestsOnTenantSubdomain()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        using var client = Factory.CreateClientFor(tenant.Identifier);

        var response = await client.GetAsync("openapi/v1.json");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }
}
