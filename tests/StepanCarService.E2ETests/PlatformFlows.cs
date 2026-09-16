using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;

namespace StepanCarService.E2ETests;

// Общие шаги сквозных сценариев: регистрация владельца, создание тенанта, ожидание репликации в остальные сервисы
internal static class PlatformFlows
{
    public static readonly TimeSpan ReplicationTimeout = TimeSpan.FromSeconds(30);

    public static async Task<(TenantReadDto Tenant, string OwnerPhone, string OwnerPassword, string OwnerToken)> OnboardTenantAsync(this PlatformFixture platform)
    {
        var phone = TestData.Phone();
        var password = TestData.Password();

        using var portal = platform.Auth.CreateClientFor();
        var register = await portal.PostAsJsonAsync("api/Auth/register",
            new RegisterRequestDto(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);
        var ownerToken = await portal.LoginAsync(phone, password);

        using var tenantApi = platform.Tenant.CreateClientFor().WithBearer(ownerToken);
        var create = await tenantApi.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), TestData.CompanyName()));
        await create.ShouldBeStatusAsync(HttpStatusCode.OK);
        var tenant = (await create.Content.ReadFromJsonAsync<TenantReadDto>()).ShouldNotBeNull();

        await platform.WaitForTenantReplicationAsync(tenant.Id);
        return (tenant, phone, password, ownerToken);
    }

    public static async Task WaitForTenantReplicationAsync(this PlatformFixture platform, string tenantId)
    {
        await Eventually.WaitUntilAsync(() => platform.Auth.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenantId)),
            ReplicationTimeout, because: "тенант должен появиться в Auth");
        await Eventually.WaitUntilAsync(() => platform.Detail.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenantId)),
            ReplicationTimeout, because: "тенант должен появиться в Detail");
        await Eventually.WaitUntilAsync(() => platform.Visit.WithDbContextAsync(db => db.Tenants.AnyAsync(t => t.Id == tenantId)),
            ReplicationTimeout, because: "тенант должен появиться в Visit");
    }

    public static async Task<string> RegisterTenantUserAsync(this HttpClient tenantSubdomainClient, string phone, string password)
    {
        var register = await tenantSubdomainClient.PostAsJsonAsync("api/Auth/register",
            new RegisterRequestDto(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);
        return await tenantSubdomainClient.LoginAsync(phone, password);
    }

    public static async Task<string> LoginAsync(this HttpClient client, string phone, string password)
    {
        var login = await client.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(phone, password));
        await login.ShouldBeStatusAsync(HttpStatusCode.OK);
        return (await login.Content.ReadFromJsonAsync<AuthResponseDto>()).ShouldNotBeNull().AccessToken;
    }
}
