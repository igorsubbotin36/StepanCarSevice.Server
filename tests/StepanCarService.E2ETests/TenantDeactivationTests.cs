using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.E2ETests;

// Деактивация тенанта за неуплату (E2E-03): GodMode отключает подписку → поддомен закрыт владельцу
// и пользователю тенанта, тенант исчезает из публичного каталога, но виден владельцу на портале;
// активация возвращает доступ
[Trait(TestCategories.Name, TestCategories.E2E)]
[Collection(PlatformCollection.Name)]
public class TenantDeactivationTests(PlatformFixture platform)
{
    [Fact]
    public async Task DeactivatingTenant_BlocksSubdomainAndHidesFromCatalog_ReactivationRestoresAccess()
    {
        var (tenant, ownerPhone, ownerPassword, ownerToken) = await platform.OnboardTenantAsync();
        var userPhone = TestData.Phone();
        var userPassword = TestData.Password();

        using var subdomain = platform.Auth.CreateClientFor(tenant.Identifier);
        await subdomain.RegisterTenantUserAsync(userPhone, userPassword);

        using var portal = platform.Tenant.CreateClientFor();
        (await GetCatalogAsync(portal)).ShouldContain(t => t.Identifier == tenant.Identifier);

        using var godMode = platform.Tenant.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.GodMode));
        var deactivate = await godMode.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(tenant.Id, tenant.Name, false));
        await deactivate.ShouldBeStatusAsync(HttpStatusCode.OK);

        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenant.Id && !t.IsActive)),
            PlatformFlows.ReplicationTimeout, because: "деактивация должна реплицироваться в Auth");

        (await GetCatalogAsync(portal)).ShouldNotContain(t => t.Identifier == tenant.Identifier);

        var ownerLogin = await subdomain.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(ownerPhone, ownerPassword));
        await ownerLogin.ShouldBeErrorAsync(HttpStatusCode.Forbidden, TenantErrors.TenantInactive);
        var userLogin = await subdomain.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(userPhone, userPassword));
        await userLogin.ShouldBeErrorAsync(HttpStatusCode.Forbidden, TenantErrors.TenantInactive);

        // владелец по-прежнему видит свой тенант на портале, несмотря на деактивацию
        using var ownerPortal = platform.Tenant.CreateClientFor().WithBearer(ownerToken);
        await (await ownerPortal.GetAsync("api/Tenant/getMyTenant")).ShouldBeStatusAsync(HttpStatusCode.OK);

        var reactivate = await godMode.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(tenant.Id, tenant.Name, true));
        await reactivate.ShouldBeStatusAsync(HttpStatusCode.OK);
        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenant.Id && t.IsActive)),
            PlatformFlows.ReplicationTimeout, because: "активация должна реплицироваться в Auth");

        var ownerLoginAfter = await subdomain.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(ownerPhone, ownerPassword));
        await ownerLoginAfter.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    private static async Task<List<ConnectedTenantDto>> GetCatalogAsync(HttpClient portal) =>
        (await portal.GetFromJsonAsync<List<ConnectedTenantDto>>("api/Tenant/getConnectedTenants")).ShouldNotBeNull();
}
