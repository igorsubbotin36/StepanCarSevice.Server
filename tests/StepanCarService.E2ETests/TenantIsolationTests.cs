using System.Net;
using System.Net.Http.Json;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;

namespace StepanCarService.E2ETests;

// Изоляция двух автосервисов (E2E-02): токены и логины одного тенанта не работают в другом,
// одинаковый телефон независим в разных тенантах. Изоляция данных Detail/Visit на уровне БД
// уже исчерпывающе проверена в StepanCarService.DetailService.Tests/VisitService.Tests (фаза 3);
// здесь — то же самое поведение через реальный HTTP на двух реплицированных тенантах
[Trait(TestCategories.Name, TestCategories.E2E)]
[Collection(PlatformCollection.Name)]
public class TenantIsolationTests(PlatformFixture platform)
{
    [Fact]
    public async Task TwoTenants_TokensAndPhonesAreIndependent()
    {
        var (tenantA, _, _, ownerTokenA) = await platform.OnboardTenantAsync();
        var (tenantB, _, _, _) = await platform.OnboardTenantAsync();

        var sharedPhone = TestData.Phone();
        var passwordA = TestData.Password();
        var passwordB = TestData.Password();

        using var subdomainA = platform.Auth.CreateClientFor(tenantA.Identifier);
        using var subdomainB = platform.Auth.CreateClientFor(tenantB.Identifier);

        // один и тот же телефон — независимые учётные записи в разных тенантах
        var tokenA = await subdomainA.RegisterTenantUserAsync(sharedPhone, passwordA);
        var tokenB = await subdomainB.RegisterTenantUserAsync(sharedPhone, passwordB);

        // пароль учётки другого тенанта не подходит на своём поддомене
        var wrongPassword = await subdomainA.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(sharedPhone, passwordB));
        await wrongPassword.ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);

        // токен пользователя A не работает на поддомене B и наоборот (несовпадение tenant_id)
        using var crossToB = platform.Auth.CreateClientFor(tenantB.Identifier).WithBearer(tokenA);
        (await crossToB.GetAsync("api/Auth/getTokenClaims")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var crossToA = platform.Auth.CreateClientFor(tenantA.Identifier).WithBearer(tokenB);
        (await crossToA.GetAsync("api/Auth/getTokenClaims")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // свой токен на своём поддомене по-прежнему работает
        using var ownToA = platform.Auth.CreateClientFor(tenantA.Identifier).WithBearer(tokenA);
        (await ownToA.GetAsync("api/Auth/getTokenClaims")).StatusCode.ShouldBe(HttpStatusCode.OK);

        // владелец A не признаётся владельцем чужого тенанта B (Tenant-сервис — только портал, поддомен роли не играет)
        using var tenantApiAsOwnerA = platform.Tenant.CreateClientFor().WithBearer(ownerTokenA);
        var getTenantB = await tenantApiAsOwnerA.GetAsync($"api/Tenant/getTenantById?id={tenantB.Id}");
        await getTenantB.ShouldBeErrorAsync(HttpStatusCode.Forbidden, AuthErrors.Forbidden);
    }
}
