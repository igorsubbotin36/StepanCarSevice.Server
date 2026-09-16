using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.TenantService.Tests.Security;

[Trait(TestCategories.Name, TestCategories.Security)]
[Collection(TestCollections.Database)]
public class TenantSecurityTests(TenantServiceFactory factory)
    : ApiTestBase<TenantServiceFactory, Program, TenantServiceDbContext>(factory)
{
    private const int OwnerId = 7;
    private const int OtherOwnerId = 8;

    private static readonly string[] TenantReadFields = ["id", "identifier", "isActive", "name", "ownerUserId"];

    private Task<TenantInfoEntity> SeedTenantAsync(TenantBuilder builder) => Factory.WithDbContextAsync(async db =>
    {
        var tenant = builder.Build();
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    });

    private Task<TenantInfoEntity?> FindTenantAsync(string id) =>
        Factory.WithDbContextAsync(db => db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id));

    private HttpClient OwnerClient(int ownerId = OwnerId) =>
        Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.TenantOwner, userId: ownerId));

    [Theory]
    [InlineData("alg: none")]
    [InlineData("HS512 тем же ключом")]
    [InlineData("роль повышена без переподписи")]
    public async Task ForgedToken_Returns401(string forgery)
    {
        var token = forgery switch
        {
            "alg: none" => TestTokenFactory.CreateUnsigned(Roles.GodMode),
            "HS512 тем же ключом" => TestTokenFactory.Create(Roles.GodMode, algorithm: SecurityAlgorithms.HmacSha512),
            _ => ElevateRole(TestTokenFactory.Create(Roles.TenantOwner, userId: OwnerId))
        };
        using var client = Factory.CreateClientFor().WithBearer(token);

        (await client.GetAsync("api/Tenant/getAllTenants")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized, forgery);
    }

    // Владелец не видит, не меняет и не удаляет чужой тенант (и тенант без владельца) по Id
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Owner_CannotAccessForeignTenantById(bool foreignHasOwner)
    {
        var builder = TenantBuilder.Tenant().WithName("Чужой");
        if (foreignHasOwner)
            builder.OwnedBy(OtherOwnerId);
        var foreign = await SeedTenantAsync(builder);
        using var client = OwnerClient();

        await (await client.GetAsync($"api/Tenant/getTenantById?id={foreign.Id}")).ShouldBeErrorAsync(HttpStatusCode.Forbidden, AuthErrors.Forbidden);
        await (await client.PatchAsJsonAsync("api/Tenant/updateTenant", new TenantUpdateDto(foreign.Id!, "Захвачен", null)))
            .ShouldBeErrorAsync(HttpStatusCode.Forbidden, AuthErrors.Forbidden);
        await (await client.DeleteAsync($"api/Tenant/deleteTenant?id={foreign.Id}")).ShouldBeErrorAsync(HttpStatusCode.Forbidden, AuthErrors.Forbidden);

        var stored = await FindTenantAsync(foreign.Id!);
        stored.ShouldNotBeNull().Name.ShouldBe("Чужой");
        (await Factory.WithDbContextAsync(db => db.OutboxMessages.CountAsync())).ShouldBe(0);
    }

    // getMyTenant возвращает только тенант вызывающего
    [Fact]
    public async Task GetMyTenant_ReturnsOnlyCallersTenant()
    {
        await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OtherOwnerId));
        using var client = OwnerClient();

        await (await client.GetAsync("api/Tenant/getMyTenant")).ShouldBeErrorAsync(HttpStatusCode.NotFound, TenantErrors.TenantNotFound);
    }

    // Id, владелец и активность при создании определяет сервер
    [Fact]
    public async Task RegisterTenant_ExtraFieldsInBody_AreIgnored()
    {
        using var client = OwnerClient();
        var identifier = TestData.TenantIdentifier();

        var response = await client.PostAsJsonAsync("api/Tenant/registerTenant", new
        {
            identifier,
            name = "Сервис",
            id = "chosen-by-client",
            ownerUserId = OtherOwnerId,
            isActive = false,
            apiKey = "key",
            connectionString = "Host=evil"
        });

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var stored = await Factory.WithDbContextAsync(db => db.Tenants.AsNoTracking().SingleAsync(t => t.Identifier == identifier));
        stored.Id.ShouldNotBe("chosen-by-client");
        Guid.TryParse(stored.Id, out _).ShouldBeTrue();
        stored.OwnerUserId.ShouldBe(OwnerId);
        stored.IsActive.ShouldBeTrue();
    }

    // При изменении нельзя сменить владельца и идентификатор (поддомен)
    [Fact]
    public async Task UpdateTenant_ExtraFieldsInBody_AreIgnored()
    {
        var tenant = await SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(OwnerId));
        using var client = OwnerClient();

        var response = await client.PatchAsJsonAsync("api/Tenant/updateTenant", new
        {
            id = tenant.Id,
            name = "Новое имя",
            identifier = "hijacked",
            ownerUserId = OtherOwnerId
        });

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var stored = await FindTenantAsync(tenant.Id!);
        stored!.Name.ShouldBe("Новое имя");
        stored.Identifier.ShouldBe(tenant.Identifier);
        stored.OwnerUserId.ShouldBe(OwnerId);
    }

    // Ответы содержат только публичные поля тенанта
    [Fact]
    public async Task Responses_ContainOnlyExpectedTenantFields()
    {
        using var owner = OwnerClient();
        var created = JsonNode.Parse(await (await owner.PostAsJsonAsync("api/Tenant/registerTenant",
            new TenantCreateDto(TestData.TenantIdentifier(), "Сервис"))).Content.ReadAsStringAsync())!.AsObject();
        using var godMode = Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(Roles.GodMode));
        var all = JsonNode.Parse(await (await godMode.GetAsync("api/Tenant/getAllTenants")).Content.ReadAsStringAsync())!.AsArray();
        using var anonymous = Factory.CreateClientFor();
        var catalog = JsonNode.Parse(await (await anonymous.GetAsync("api/Tenant/getConnectedTenants")).Content.ReadAsStringAsync())!.AsArray();

        created.Select(p => p.Key).Order().ShouldBe(TenantReadFields);
        all.ShouldHaveSingleItem()!.AsObject().Select(p => p.Key).Order().ShouldBe(TenantReadFields);
        catalog.ShouldHaveSingleItem()!.AsObject().Select(p => p.Key).Order().ShouldBe(["identifier", "name"]);
    }

    private static string ElevateRole(string token)
    {
        var parts = token.Split('.');
        var payload = Base64UrlEncoder.Decode(parts[1]).Replace($"\"{Roles.TenantOwner}\"", $"\"{Roles.GodMode}\"");
        return $"{parts[0]}.{Base64UrlEncoder.Encode(payload)}.{parts[2]}";
    }
}
