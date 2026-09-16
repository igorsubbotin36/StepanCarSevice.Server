using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarService.AuthService.Tests.Api;

[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class AuthControllerTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    // AU-102
    [Fact]
    public async Task GetTokenClaims_WithoutToken_Returns401()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("api/Auth/getTokenClaims");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // AU-40, AU-55: владелец регистрируется на портале и входит; токен принимается сервисом
    [Fact]
    public async Task RegisterAndLogin_OnPortal_ReturnsWorkingToken()
    {
        using var client = Factory.CreateClientFor();
        var phone = TestData.Phone();
        var password = TestData.Password();

        var register = await client.PostAsJsonAsync("api/Auth/register",
            new RegisterRequestDto(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);
        // Сервис пишет в тестовую БД фабрики, а не в БД из user-secrets разработчика
        await using (var db = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(Factory.ConnectionString).Options))
            (await db.Users.AnyAsync(u => u.Phone == phone && u.TenantId == null)).ShouldBeTrue();

        var login = await client.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(phone, password));
        await login.ShouldBeStatusAsync(HttpStatusCode.OK);
        var token = (await login.Content.ReadFromJsonAsync<AuthResponseDto>()).ShouldNotBeNull().AccessToken;

        var claims = await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims");
        await claims.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await claims.Content.ReadAsStringAsync()).ShouldContain("TenantOwner");
    }
}
