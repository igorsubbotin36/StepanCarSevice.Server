using System.Net;
using System.Net.Http.Json;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using static StepanCarService.AuthService.Tests.AuthTestUsers;

namespace StepanCarService.AuthService.Tests.Api;

// Смена пароля, изменение профиля и списки пользователей
[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class UserControllerTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    private const string NewPassword = "New-password-1";

    private static ChangePasswordRequestDto ChangePassword(string oldPassword = Password) => new(oldPassword, NewPassword, NewPassword);

    private async Task<(TenantInfoEntity Tenant, User User)> SeedTenantUserAsync(int roleId = RoleIds.User)
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        return (tenant, await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id).WithRole(roleId)));
    }

    [Fact]
    public async Task ChangePassword_WrongOldPassword_Returns400AndKeepsPasswordAndStamp()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        var response = await client.PutAsJsonAsync("api/User/changePassword", ChangePassword("Wrong-password"));

        await response.ShouldBeErrorAsync(HttpStatusCode.BadRequest, UserErrors.WrongPassword);
        var stored = await Factory.FindUserAsync(user.Id);
        stored!.Password.ShouldBe(user.Password);
        stored.SecurityStamp.ShouldBe(user.SecurityStamp);
    }

    // Смена пароля отзывает старые токены в Auth и выдаёт новый
    [Fact]
    public async Task ChangePassword_Success_RevokesOldTokenAndReturnsNewOne()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier);
        var oldToken = await (await client.LoginAsync(user.Phone!)).ReadAccessTokenAsync();

        var response = await client.WithBearer(oldToken).PutAsJsonAsync("api/User/changePassword", ChangePassword());
        var newToken = await response.ReadAccessTokenAsync();

        var stored = await Factory.FindUserAsync(user.Id);
        stored!.SecurityStamp.ShouldNotBe(user.SecurityStamp);
        new PasswordHasher().Verify(NewPassword, stored.Password).ShouldBeTrue();
        (await client.WithBearer(oldToken).GetAsync("api/Auth/getTokenClaims")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await (await client.WithBearer(newToken).GetAsync("api/Auth/getTokenClaims")).ShouldBeStatusAsync(HttpStatusCode.OK);
        await (await client.WithBearer(null).LoginAsync(user.Phone!, NewPassword)).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Подделанный токен с Id пользователя другого тенанта (tenant_id и метка верные) не даёт доступ к его учётке
    [Fact]
    public async Task ChangePasswordAndUpdate_UserOfOtherTenant_Return404()
    {
        var (tenantA, _) = await SeedTenantUserAsync();
        var (_, victim) = await SeedTenantUserAsync();
        var forged = TestTokenFactory.Create(Roles.User, victim.Id, tenantA.Id, victim.SecurityStamp);
        using var client = Factory.CreateClientFor(tenantA.Identifier).WithBearer(forged);

        await (await client.PutAsJsonAsync("api/User/changePassword", ChangePassword()))
            .ShouldBeErrorAsync(HttpStatusCode.NotFound, UserErrors.NotFound);
        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto("Взлом", null, null, null)))
            .ShouldBeErrorAsync(HttpStatusCode.NotFound, UserErrors.NotFound);
        (await Factory.FindUserAsync(victim.Id))!.FirstName.ShouldBe(victim.FirstName);
    }

    // Пользователь портала с чужим tenant_id в токене — тоже 404
    [Fact]
    public async Task ChangePassword_TokenWithIdOfPortalOwner_Returns404()
    {
        var (tenant, _) = await SeedTenantUserAsync();
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var forged = TestTokenFactory.Create(Roles.User, owner.Id, tenant.Id, owner.SecurityStamp);
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(forged);

        await (await client.PutAsJsonAsync("api/User/changePassword", ChangePassword()))
            .ShouldBeErrorAsync(HttpStatusCode.NotFound, UserErrors.NotFound);
    }

    [Fact]
    public async Task ChangePassword_OwnerOnOwnTenant_Returns200()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(owner.Id));
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(owner, Roles.TenantOwner));

        await (await client.PutAsJsonAsync("api/User/changePassword", ChangePassword())).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Без токена и с невалидным телом
    [Fact]
    public async Task ChangePassword_WithoutTokenOrInvalidBody_IsRejected()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier);

        (await client.PutAsJsonAsync("api/User/changePassword", ChangePassword())).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.WithBearer(TokenFor(user, Roles.User)).PutAsJsonAsync("api/User/changePassword", new ChangePasswordRequestDto(Password, Password, Password)))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_PartialRequest_ChangesOnlyProvidedFields()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto("Новое имя", null, null, null)))
            .ShouldBeStatusAsync(HttpStatusCode.OK);

        var stored = await Factory.FindUserAsync(user.Id);
        stored!.FirstName.ShouldBe("Новое имя");
        stored.SecondName.ShouldBe(user.SecondName);
        stored.Email.ShouldBe(user.Email);
        stored.Phone.ShouldBe(user.Phone);
    }

    [Fact]
    public async Task UpdateUser_PhoneUniqueWithinTenantOnly()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        var neighbour = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));
        var (_, stranger) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto(null, null, null, neighbour.Phone)))
            .ShouldBeErrorAsync(HttpStatusCode.Conflict, RegisterErrors.UserAlreadyExists);
        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto(null, null, null, stranger.Phone)))
            .ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Привязку к тенанту, роль и Id нельзя передать в теле
    [Fact]
    public async Task UpdateUser_ExtraFieldsInBody_AreIgnored()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        var (otherTenant, _) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        var response = await client.PatchAsJsonAsync("api/User/updateUser", new
        {
            firstName = "Имя",
            tenantId = otherTenant.Id,
            roleId = RoleIds.GodMode,
            role = Roles.GodMode,
            id = user.Id + 1000,
            securityStamp = "forged",
            password = "plain"
        });

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var stored = await Factory.FindUserAsync(user.Id);
        stored!.FirstName.ShouldBe("Имя");
        stored.TenantId.ShouldBe(tenant.Id);
        stored.RoleId.ShouldBe(RoleIds.User);
        stored.SecurityStamp.ShouldBe(user.SecurityStamp);
        stored.Password.ShouldBe(user.Password);
    }

    // Пользователь идентифицируется по Id, а не по телефону из токена
    [Fact]
    public async Task UpdateUser_PhoneChanged_SameTokenStillWorks()
    {
        var (tenant, user) = await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));

        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto(null, null, null, TestData.Phone())))
            .ShouldBeStatusAsync(HttpStatusCode.OK);

        await (await client.GetAsync("api/Auth/getTokenClaims")).ShouldBeStatusAsync(HttpStatusCode.OK);
        await (await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto("Ещё раз", null, null, null)))
            .ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Одновременная смена на один телефон — 409, а не 500
    [Fact]
    public async Task UpdateUser_ParallelSamePhone_OneSucceedsOthersConflict()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var users = new List<User>();
        for (var i = 0; i < 6; i++)
            users.Add(await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id)));
        var phone = TestData.Phone();

        var responses = await Task.WhenAll(users.Select(async user =>
        {
            using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(user, Roles.User));
            return await client.PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto(null, null, null, phone));
        }));

        responses.Select(r => r.StatusCode).ShouldAllBe(s => s == HttpStatusCode.OK || s == HttpStatusCode.Conflict);
        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
    }

    [Fact(Skip = "Баг: для отсутствующего телефона возвращается 400 USER_INVALID_PHONE вместо 404 USER_NOT_FOUND")]
    public async Task GetUserInfo_UnknownPhone_Returns404()
    {
        var godMode = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.GodMode));
        using var client = Factory.CreateClientFor().WithBearer(TokenFor(godMode, Roles.GodMode));

        var response = await client.GetAsync($"api/User/getUserInfo?phone={Uri.EscapeDataString(TestData.Phone())}");

        await response.ShouldBeErrorAsync(HttpStatusCode.NotFound, UserErrors.NotFound);
    }

    [Fact]
    public async Task GetAllUsers_InTenant_ReturnsOnlyTenantUsersWithRoles()
    {
        var (tenant, moderator) = await SeedTenantUserAsync(RoleIds.TenantModerator);
        var user = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));
        await SeedTenantUserAsync();
        await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(moderator, Roles.TenantModerator));

        var response = await client.GetAsync("api/User/getAllUsers");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserInfoDto>>();
        users!.Select(u => (u.Phone, u.Role)).ShouldBe(
            [(moderator.Phone, Roles.TenantModerator), (user.Phone, Roles.User)], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllUsers_GodModeOnPortal_ReturnsOnlyPortalUsers()
    {
        var godMode = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.GodMode));
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        await SeedTenantUserAsync();
        using var client = Factory.CreateClientFor().WithBearer(TokenFor(godMode, Roles.GodMode));

        var users = await (await client.GetAsync("api/User/getAllUsers")).Content.ReadFromJsonAsync<List<UserInfoDto>>();

        users!.Select(u => u.Phone).ShouldBe([godMode.Phone, owner.Phone], ignoreOrder: true);
    }

    // Права на список пользователей тенанта
    [Theory]
    [InlineData(RoleIds.User, HttpStatusCode.Forbidden)]
    [InlineData(RoleIds.TenantModerator, HttpStatusCode.OK)]
    [InlineData(RoleIds.TenantOwner, HttpStatusCode.OK)]
    [InlineData(RoleIds.GodMode, HttpStatusCode.OK)]
    public async Task GetAllUsers_RoleMatrixInTenant(int roleId, HttpStatusCode expected)
    {
        var caller = await Factory.SeedUserAsync(UserBuilder.User().WithRole(roleId));
        var tenantBuilder = TenantBuilder.Tenant();
        if (roleId == RoleIds.TenantOwner)
            tenantBuilder.OwnedBy(caller.Id);
        var tenant = await Factory.SeedTenantAsync(tenantBuilder);
        var token = roleId switch
        {
            RoleIds.User => TestTokenFactory.Create(Roles.User, caller.Id, tenant.Id, caller.SecurityStamp),
            RoleIds.TenantModerator => TestTokenFactory.Create(Roles.TenantModerator, caller.Id, tenant.Id, caller.SecurityStamp),
            RoleIds.TenantOwner => TokenFor(caller, Roles.TenantOwner),
            _ => TokenFor(caller, Roles.GodMode)
        };
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(token);

        await (await client.GetAsync("api/User/getAllUsers")).ShouldBeStatusAsync(expected);
    }

    [Theory]
    [InlineData(RoleIds.TenantOwner, HttpStatusCode.Forbidden)]
    [InlineData(RoleIds.GodMode, HttpStatusCode.OK)]
    public async Task GetUserInfo_OnlyGodMode(int roleId, HttpStatusCode expected)
    {
        var caller = await Factory.SeedUserAsync(UserBuilder.User().WithRole(roleId));
        var role = roleId == RoleIds.GodMode ? Roles.GodMode : Roles.TenantOwner;
        using var client = Factory.CreateClientFor().WithBearer(TokenFor(caller, role));

        var response = await client.GetAsync($"api/User/getUserInfo?phone={Uri.EscapeDataString(caller.Phone!)}");

        await response.ShouldBeStatusAsync(expected);
    }

    // Модератор и пользователь в своём тенанте
    [Theory]
    [InlineData(RoleIds.TenantModerator, Roles.TenantModerator)]
    [InlineData(RoleIds.User, Roles.User)]
    public async Task GetUserInfo_TenantRoles_Return403(int roleId, string role)
    {
        var (tenant, caller) = await SeedTenantUserAsync(roleId);
        using var client = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(caller, role));

        (await client.GetAsync($"api/User/getUserInfo?phone={Uri.EscapeDataString(caller.Phone!)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
