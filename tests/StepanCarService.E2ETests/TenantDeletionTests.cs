using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;

namespace StepanCarService.E2ETests;

// Удаление автосервиса (E2E-04): владелец удаляет тенант → пользователи тенанта удалены в Auth,
// копия тенанта удалена в Detail и Visit, каталог его больше не показывает, а владелец может создать новый тенант
[Trait(TestCategories.Name, TestCategories.E2E)]
[Collection(PlatformCollection.Name)]
public class TenantDeletionTests(PlatformFixture platform)
{
    [Fact]
    public async Task OwnerDeletesTenant_UsersAndTenantCopyRemovedEverywhere_OwnerCanCreateNewTenant()
    {
        var (tenant, _, _, ownerToken) = await platform.OnboardTenantAsync();
        var userPhone = TestData.Phone();

        using var subdomain = platform.Auth.CreateClientFor(tenant.Identifier);
        await subdomain.RegisterTenantUserAsync(userPhone, TestData.Password());

        using var tenantApi = platform.Tenant.CreateClientFor().WithBearer(ownerToken);
        var delete = await tenantApi.DeleteAsync($"api/Tenant/deleteTenant?id={tenant.Id}");
        await delete.ShouldBeStatusAsync(HttpStatusCode.OK);

        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(async db => !await db.Tenants.AnyAsync(t => t.Id == tenant.Id)),
            PlatformFlows.ReplicationTimeout, because: "тенант должен быть удалён в Auth");
        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(async db => !await db.Users.AnyAsync(u => u.Phone == userPhone)),
            PlatformFlows.ReplicationTimeout, because: "пользователи тенанта должны быть удалены каскадом в Auth");
        await Eventually.WaitUntilAsync(() => platform.Detail.WithDbContextAsync(async db => !await db.Tenants.AnyAsync(t => t.Id == tenant.Id)),
            PlatformFlows.ReplicationTimeout, because: "тенант должен быть удалён в Detail");
        await Eventually.WaitUntilAsync(() => platform.Visit.WithDbContextAsync(async db => !await db.Tenants.AnyAsync(t => t.Id == tenant.Id)),
            PlatformFlows.ReplicationTimeout, because: "тенант должен быть удалён в Visit");

        using var portal = platform.Tenant.CreateClientFor();
        var catalog = (await portal.GetFromJsonAsync<List<ConnectedTenantDto>>("api/Tenant/getConnectedTenants")).ShouldNotBeNull();
        catalog.ShouldNotContain(t => t.Identifier == tenant.Identifier);

        // владелец может создать новый тенант тем же токеном — уникальный индекс по OwnerUserId освобождён
        var create = await tenantApi.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), TestData.CompanyName()));
        await create.ShouldBeStatusAsync(HttpStatusCode.OK);
    }
}
