using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarService.AuthService.Tests.Api;

// Регистр идентификатора тенанта в поддомене и поведение неизвестного поддомена
[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class MultiTenantHostResolutionTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    private static RegisterRequestDto RegisterRequest(string phone, string password) =>
        new(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password);

    // TENANT1.localhost и tenant1.localhost — один и тот же тенант: регистрация на верхнем регистре
    // должна создать пользователя тенанта, а не пользователя портала
    [Fact]
    public async Task Subdomain_DifferentCase_ResolvesSameTenant()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        using var client = Factory.CreateClientFor(tenant.Identifier!.ToUpperInvariant());
        var phone = TestData.Phone();
        var password = TestData.Password();

        var register = await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(phone, password));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);

        await using var db = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(Factory.ConnectionString).Options);
        (await db.Users.SingleAsync(u => u.Phone == phone)).TenantId.ShouldBe(tenant.Id);
    }

    // Поддомен известного портального домена, для которого нет тенанта, — 404, а не поведение портала
    [Fact]
    public async Task UnknownSubdomain_Returns404()
    {
        using var client = Factory.CreateClientFor(TestData.TenantIdentifier("nosuch"));

        var response = await client.GetAsync("api/Auth/getTokenClaims");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // Сам портал (без поддомена) не должен задеваться новой проверкой неизвестного поддомена
    [Fact]
    public async Task Portal_WithoutSubdomain_StillBehavesAsPortal()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("api/Auth/getTokenClaims");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
