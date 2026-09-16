using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;

namespace StepanCarService.E2ETests;

[Trait(TestCategories.Name, TestCategories.E2E)]
[Collection(PlatformCollection.Name)]
public class TenantOnboardingTests(PlatformFixture platform)
{
    private static readonly TimeSpan ReplicationTimeout = TimeSpan.FromSeconds(30);

    // Владелец создаёт тенант, тенант реплицируется во все сервисы, владелец входит на свой поддомен
    [Fact]
    public async Task OwnerCreatesTenant_TenantReplicatedAndOwnerLogsInOnSubdomain()
    {
        var phone = TestData.Phone();
        var password = TestData.Password();
        var identifier = TestData.TenantIdentifier();

        using var portal = platform.Auth.CreateClientFor();
        var register = await portal.PostAsJsonAsync("api/Auth/register",
            new RegisterRequestDto(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);
        var token = await LoginAsync(portal, phone, password);

        using var tenantApi = platform.Tenant.CreateClientFor().WithBearer(token);
        var create = await tenantApi.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(identifier, TestData.CompanyName()));
        await create.ShouldBeStatusAsync(HttpStatusCode.OK);
        var tenant = (await create.Content.ReadFromJsonAsync<TenantReadDto>()).ShouldNotBeNull();
        tenant.OwnerUserId.ShouldNotBeNull();

        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenant.Id && t.OwnerUserId == tenant.OwnerUserId)),
            ReplicationTimeout, because: "тенант должен появиться в Auth");
        await Eventually.WaitUntilAsync(() => platform.Detail.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenant.Id)),
            ReplicationTimeout, because: "тенант должен появиться в Detail");
        await Eventually.WaitUntilAsync(() => platform.Visit.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenant.Id)),
            ReplicationTimeout, because: "тенант должен появиться в Visit");

        using var subdomain = platform.Auth.CreateClientFor(identifier);
        await LoginAsync(subdomain, phone, password);
    }

    private static async Task<string> LoginAsync(HttpClient client, string phone, string password)
    {
        var login = await client.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(phone, password));
        await login.ShouldBeStatusAsync(HttpStatusCode.OK);
        return (await login.Content.ReadFromJsonAsync<AuthResponseDto>()).ShouldNotBeNull().AccessToken;
    }
}
