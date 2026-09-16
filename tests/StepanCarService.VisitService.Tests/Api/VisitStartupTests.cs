using System.Net;
using StepanCarSevice.VisitService.API;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;

namespace StepanCarService.VisitService.Tests.Api;

[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class VisitStartupTests(VisitServiceFactory factory)
    : ApiTestBase<VisitServiceFactory, Program, VisitDBContext>(factory)
{
    // Сервис стартует на пустой БД и отвечает на запросы
    [Fact]
    public async Task Service_StartsAndServesOpenApi()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("openapi/v1.json");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }
}
