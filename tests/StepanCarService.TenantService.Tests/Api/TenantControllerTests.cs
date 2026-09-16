using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.TenantService.Tests.Api;

[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class TenantControllerTests(TenantServiceFactory factory)
    : ApiTestBase<TenantServiceFactory, Program, TenantServiceDbContext>(factory)
{
    private const int OwnerId = 42;

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

    [Fact]
    public async Task RegisterTenant_Success_CreatesTenantAndOutboxEvent()
    {
        using var client = OwnerClient();
        var identifier = TestData.TenantIdentifier();

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(identifier, "Сервис"));

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await FindTenantByIdentifierAsync(identifier)).ShouldNotBeNull();
        (await Factory.WithDbContextAsync(db => db.OutboxMessages.CountAsync())).ShouldBe(1);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("management")]
    [InlineData("Invalid_ID")]
    public async Task RegisterTenant_InvalidIdentifier_Returns400(string identifier)
    {
        using var client = OwnerClient();

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(identifier, "Сервис"));

        await response.ShouldBeErrorAsync(HttpStatusCode.BadRequest, TenantErrors.InvalidIdentifier);
    }

    [Fact]
    public async Task RegisterTenant_BlankName_Returns400()
    {
        using var client = OwnerClient();

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), "   "));

        await response.ShouldBeErrorAsync(HttpStatusCode.BadRequest, ValidationErrors.RequiredField);
    }

    [Fact]
    public async Task RegisterTenant_DuplicateIdentifier_Returns409()
    {
        var identifier = TestData.TenantIdentifier();
        await SeedTenantAsync(TenantBuilder.Tenant().WithIdentifier(identifier));
        using var client = OwnerClient();

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(identifier, "Другой сервис"));

        await response.ShouldBeErrorAsync(HttpStatusCode.Conflict, TenantErrors.TenantAlreadyExists);
    }

    [Fact]
    public async Task RegisterTenant_OwnerAlreadyHasTenant_Returns409()
    {
        using var client = OwnerClient();
        await (await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), "Первый")))
            .ShouldBeStatusAsync(HttpStatusCode.OK);

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), "Второй"));

        await response.ShouldBeErrorAsync(HttpStatusCode.Conflict, TenantErrors.OwnerAlreadyHasTenant);
    }

    [Fact]
    public async Task UpdateTenant_UnknownId_Returns404()
    {
        using var client = OwnerClient();

        var response = await client.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(Guid.NewGuid().ToString(), "Имя", null));

        await response.ShouldBeErrorAsync(HttpStatusCode.NotFound, TenantErrors.TenantNotFound);
    }

    [Fact]
    public async Task UpdateTenant_OwnerChangesIsActive_Returns403AndLeavesUnchanged()
    {
        var tenant = await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OwnerId));
        using var client = OwnerClient();

        var response = await client.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(tenant.Id!, tenant.Name!, IsActive: false));

        await response.ShouldBeErrorAsync(HttpStatusCode.Forbidden, AuthErrors.Forbidden);
        (await FindTenantByIdAsync(tenant.Id!))!.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateTenant_GodModeDeactivates_Returns200()
    {
        var tenant = await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OwnerId));
        using var client = Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.GodMode));

        var response = await client.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(tenant.Id!, tenant.Name!, IsActive: false));

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await FindTenantByIdAsync(tenant.Id!))!.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteTenant_UnknownId_Returns404()
    {
        using var client = OwnerClient();

        var response = await client.DeleteAsync($"api/Tenant/deleteTenant?id={Guid.NewGuid()}");

        await response.ShouldBeErrorAsync(HttpStatusCode.NotFound, TenantErrors.TenantNotFound);
    }

    [Fact]
    public async Task DeleteTenant_Owner_RemovesTenantAndPublishesEvent()
    {
        var tenant = await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OwnerId));
        using var client = OwnerClient();

        var response = await client.DeleteAsync($"api/Tenant/deleteTenant?id={tenant.Id}");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await FindTenantByIdAsync(tenant.Id!)).ShouldBeNull();
        (await Factory.WithDbContextAsync(db => db.OutboxMessages.CountAsync())).ShouldBe(1);
    }

    [Fact]
    public async Task GetTenantById_UnknownId_Returns404()
    {
        using var client = OwnerClient();

        var response = await client.GetAsync($"api/Tenant/getTenantById?id={Guid.NewGuid()}");

        await response.ShouldBeErrorAsync(HttpStatusCode.NotFound, TenantErrors.TenantNotFound);
    }

    [Fact]
    public async Task GetTenantById_Owner_ReturnsOwnTenant()
    {
        var tenant = await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OwnerId).WithName("Мой сервис"));
        using var client = OwnerClient();

        var response = await client.GetAsync($"api/Tenant/getTenantById?id={tenant.Id}");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TenantReadDto>();
        body!.Name.ShouldBe("Мой сервис");
    }

    private HttpClient OwnerClient() => Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.TenantOwner, userId: OwnerId));

    private Task<TenantInfoEntity> SeedTenantAsync(TenantBuilder builder) => Factory.WithDbContextAsync(async db =>
    {
        var tenant = builder.Build();
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    });

    private Task<TenantInfoEntity?> FindTenantByIdAsync(string id) =>
        Factory.WithDbContextAsync(db => db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id));

    private Task<TenantInfoEntity?> FindTenantByIdentifierAsync(string identifier) =>
        Factory.WithDbContextAsync(db => db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Identifier == identifier));
}
