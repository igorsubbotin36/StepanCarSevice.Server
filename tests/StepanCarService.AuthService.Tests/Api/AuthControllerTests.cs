using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using static StepanCarService.AuthService.Tests.AuthTestUsers;

namespace StepanCarService.AuthService.Tests.Api;

// Регистрация и вход: портал / поддомен тенанта, активность тенанта, владелец тенанта
[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class AuthControllerTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    private static RegisterRequestDto RegisterRequest(string phone, string password = Password) =>
        new(TestData.Email(), TestData.FirstName(), TestData.LastName(), phone, password, password);

    [Fact]
    public async Task GetTokenClaims_WithoutToken_Returns401()
    {
        using var client = Factory.CreateClientFor();

        var response = await client.GetAsync("api/Auth/getTokenClaims");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Владелец регистрируется на портале и входит; токен принимается сервисом
    [Fact]
    public async Task RegisterAndLogin_OnPortal_ReturnsWorkingOwnerToken()
    {
        using var client = Factory.CreateClientFor();
        var phone = TestData.Phone();

        var register = await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(phone));
        await register.ShouldBeStatusAsync(HttpStatusCode.OK);
        // Сервис пишет в тестовую БД фабрики, а не в БД из user-secrets разработчика
        await using (var db = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(Factory.ConnectionString).Options))
            (await db.Users.Include(u => u.Role).SingleAsync(u => u.Phone == phone)).ShouldSatisfyAllConditions(
                u => u.TenantId.ShouldBeNull(),
                u => u.Role.Name.ShouldBe(Roles.TenantOwner));

        var token = await (await client.LoginAsync(phone)).ReadAccessTokenAsync();
        ReadJwt(token).ClaimValue(AuthClaimTypes.TenantId).ShouldBeNull();

        var claims = await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims");
        await claims.ShouldBeStatusAsync(HttpStatusCode.OK);
        var body = await claims.Content.ReadFromJsonAsync<UserDto>();
        // Служебные claims исключены, типы — короткие имена
        body!.Claims.ShouldContain(c => c.Type == "role" && c.Value == Roles.TenantOwner);
        body.Claims.ShouldNotContain(c => c.Type == "exp" || c.Type == "iss" || c.Type == "aud" || c.Type == "nbf");
    }

    [Fact]
    public async Task RegisterAndLogin_OnActiveTenant_CreatesTenantUser()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        using var client = Factory.CreateClientFor(tenant.Identifier);
        var phone = TestData.Phone();

        await (await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(phone))).ShouldBeStatusAsync(HttpStatusCode.OK);
        var token = await (await client.LoginAsync(phone)).ReadAccessTokenAsync();

        var jwt = ReadJwt(token);
        jwt.ClaimValue(AuthClaimTypes.TenantId).ShouldBe(tenant.Id);
        jwt.ClaimValue(ClaimTypes.Role).ShouldBe(Roles.User);
        await (await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims")).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_OnInactiveTenant_Returns403AndCreatesNothing()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant().Inactive());
        using var client = Factory.CreateClientFor(tenant.Identifier);
        var phone = TestData.Phone();

        var response = await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(phone));

        await response.ShouldBeErrorAsync(HttpStatusCode.Forbidden, TenantErrors.TenantInactive);
        (await Factory.WithDbContextAsync(db => db.Users.AnyAsync(u => u.Phone == phone))).ShouldBeFalse();
    }

    // Невалидное тело отклоняется автовалидацией до сервиса
    [Fact]
    public async Task Register_InvalidBody_Returns400ValidationProblem()
    {
        using var client = Factory.CreateClientFor();
        var request = RegisterRequest(TestData.Phone()) with { Email = "not-an-email", ConfirmPassword = "Other-password" };

        var response = await client.PostAsJsonAsync("api/Auth/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Keys.ShouldBe(["ConfirmPassword", "Email"], ignoreOrder: true);
    }

    [Fact]
    public async Task Register_PhoneTakenInSameTenant_Returns409()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var portalUser = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var tenantUser = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));

        using var portal = Factory.CreateClientFor();
        using var tenantClient = Factory.CreateClientFor(tenant.Identifier);

        await (await portal.PostAsJsonAsync("api/Auth/register", RegisterRequest(portalUser.Phone!)))
            .ShouldBeErrorAsync(HttpStatusCode.Conflict, RegisterErrors.UserAlreadyExists);
        await (await tenantClient.PostAsJsonAsync("api/Auth/register", RegisterRequest(tenantUser.Phone!)))
            .ShouldBeErrorAsync(HttpStatusCode.Conflict, RegisterErrors.UserAlreadyExists);
    }

    // Телефон уникален в пределах тенанта
    [Fact]
    public async Task Register_PhoneTakenInOtherTenantOrPortal_Returns200()
    {
        var tenantA = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var tenantB = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var userA = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenantA.Id));
        using var clientB = Factory.CreateClientFor(tenantB.Identifier);
        using var portal = Factory.CreateClientFor();

        await (await clientB.PostAsJsonAsync("api/Auth/register", RegisterRequest(userA.Phone!))).ShouldBeStatusAsync(HttpStatusCode.OK);
        await (await portal.PostAsJsonAsync("api/Auth/register", RegisterRequest(userA.Phone!))).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Гонка на уникальном индексе даёт 409, а не 500
    [Fact]
    public async Task Register_ParallelSamePhone_ExactlyOneSucceeds()
    {
        using var client = Factory.CreateClientFor();
        var request = RegisterRequest(TestData.Phone());

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.PostAsJsonAsync("api/Auth/register", request)));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(7);
        (await Factory.WithDbContextAsync(db => db.Users.CountAsync(u => u.Phone == request.Phone))).ShouldBe(1);
    }

    [Fact]
    public async Task Register_StoresPasswordHashAndSecurityStamp()
    {
        using var client = Factory.CreateClientFor();
        var phone = TestData.Phone();

        await (await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(phone))).ShouldBeStatusAsync(HttpStatusCode.OK);

        var user = await Factory.WithDbContextAsync(db => db.Users.SingleAsync(u => u.Phone == phone));
        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
        user.Password.ShouldNotContain(Password);
        new PasswordHasher().Verify(Password, user.Password).ShouldBeTrue();
    }

    [Fact]
    public async Task Login_WrongPasswordOrUnknownPhone_Returns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        using var client = Factory.CreateClientFor();

        await (await client.LoginAsync(owner.Phone!, "Wrong-password")).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
        await (await client.LoginAsync(TestData.Phone())).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
    }

    // Пользователь тенанта существует только в своём тенанте
    [Fact]
    public async Task Login_TenantUserOnPortalOrOtherTenant_Returns401()
    {
        var tenantA = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var tenantB = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var user = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenantA.Id));

        foreach (var host in new[] { null, tenantB.Identifier })
        {
            using var client = Factory.CreateClientFor(host);
            await (await client.LoginAsync(user.Phone!)).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
        }
    }

    // На портале входят только GodMode и TenantOwner (например, модератор, заведённый без тенанта, — нет)
    [Fact]
    public async Task Login_PortalUserWithTenantRole_Returns401()
    {
        var moderator = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantModerator));
        using var client = Factory.CreateClientFor();

        await (await client.LoginAsync(moderator.Phone!)).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
    }

    // GodMode входит на портал и в любой тенант, в том числе неактивный
    [Fact]
    public async Task Login_GodMode_WorksOnPortalAndAnyTenant()
    {
        var active = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var inactive = await Factory.SeedTenantAsync(TenantBuilder.Tenant().Inactive());
        var godMode = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.GodMode));

        foreach (var host in new[] { null, active.Identifier, inactive.Identifier })
        {
            using var client = Factory.CreateClientFor(host);
            var token = await (await client.LoginAsync(godMode.Phone!)).ReadAccessTokenAsync();
            ReadJwt(token).ClaimValue(AuthClaimTypes.TenantId).ShouldBeNull();
            await (await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims")).ShouldBeStatusAsync(HttpStatusCode.OK);
        }
    }

    // Владелец входит в свой тенант логином портала, tenant_id подставляется при проверке токена
    [Fact]
    public async Task Login_OwnerOnOwnTenant_ReturnsPortalTokenWorkingInTenant()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(owner.Id));
        using var client = Factory.CreateClientFor(tenant.Identifier);

        var token = await (await client.LoginAsync(owner.Phone!)).ReadAccessTokenAsync();

        ReadJwt(token).ClaimValue(AuthClaimTypes.TenantId).ShouldBeNull();
        var claims = await (await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims")).Content.ReadFromJsonAsync<UserDto>();
        claims!.Claims.ShouldContain(c => c.Type == "tenant_id" && c.Value == tenant.Id);
    }

    [Fact]
    public async Task Login_OwnerOnForeignOrOwnerlessTenant_Returns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var otherOwner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var foreign = await Factory.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(otherOwner.Id));
        var ownerless = await Factory.SeedTenantAsync(TenantBuilder.Tenant());

        foreach (var tenant in new[] { foreign, ownerless })
        {
            using var client = Factory.CreateClientFor(tenant.Identifier);
            await (await client.LoginAsync(owner.Phone!)).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
        }
    }

    [Fact]
    public async Task Login_SamePhoneForTenantUserAndOwner_PasswordSelectsAccount()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner), "Owner-password");
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(owner.Id));
        var tenantUser = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id).WithPhone(owner.Phone!), "User-password");
        using var client = Factory.CreateClientFor(tenant.Identifier);

        var userToken = ReadJwt(await (await client.LoginAsync(owner.Phone!, "User-password")).ReadAccessTokenAsync());
        var ownerToken = ReadJwt(await (await client.LoginAsync(owner.Phone!, "Owner-password")).ReadAccessTokenAsync());

        userToken.ClaimValue(ClaimTypes.NameIdentifier).ShouldBe(tenantUser.Id.ToString());
        userToken.ClaimValue(AuthClaimTypes.TenantId).ShouldBe(tenant.Id);
        ownerToken.ClaimValue(ClaimTypes.NameIdentifier).ShouldBe(owner.Id.ToString());
        ownerToken.ClaimValue(ClaimTypes.Role).ShouldBe(Roles.TenantOwner);
    }

    // Статус тенанта раскрывается только при верном пароле
    [Fact]
    public async Task Login_OnInactiveTenant_ForbiddenOnlyWithCorrectPassword()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(owner.Id).Inactive());
        var user = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));
        using var client = Factory.CreateClientFor(tenant.Identifier);

        await (await client.LoginAsync(user.Phone!)).ShouldBeErrorAsync(HttpStatusCode.Forbidden, TenantErrors.TenantInactive);
        await (await client.LoginAsync(owner.Phone!)).ShouldBeErrorAsync(HttpStatusCode.Forbidden, TenantErrors.TenantInactive);
        await (await client.LoginAsync(user.Phone!, "Wrong-password")).ShouldBeErrorAsync(HttpStatusCode.Unauthorized, AuthErrors.InvalidCredentials);
    }

    // Токен User в своём тенанте видит свои claims
    [Fact]
    public async Task GetTokenClaims_ReturnsClaimsOfCaller()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var user = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        var claims = await (await client.GetAsync("api/Auth/getTokenClaims")).Content.ReadFromJsonAsync<UserDto>();

        claims!.Claims.ShouldContain(c => c.Type == "nameidentifier" && c.Value == user.Id.ToString());
        claims.Claims.ShouldContain(c => c.Type == "tenant_id" && c.Value == tenant.Id);
    }

    // Лимит на регистрацию и вход настраивается в сервисе (здесь — отдельный запуск с лимитом 2)
    [Fact]
    public async Task LoginAndRegister_AreRateLimited()
    {
        using var limited = Factory.WithWebHostBuilder(builder => builder.UseSetting("RateLimiting:Auth:PermitLimit", "2"));
        using var client = limited.CreateClient(new() { BaseAddress = TestKit.MultiTenancy.TenantHost.PortalAddress });

        await client.LoginAsync(TestData.Phone());
        await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(TestData.Phone()));

        await (await client.LoginAsync(TestData.Phone())).ShouldBeErrorAsync(HttpStatusCode.TooManyRequests, SystemErrors.TooManyRequests);
        await (await client.PostAsJsonAsync("api/Auth/register", RegisterRequest(TestData.Phone())))
            .ShouldBeErrorAsync(HttpStatusCode.TooManyRequests, SystemErrors.TooManyRequests);
    }
}
