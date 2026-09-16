using System.Net;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using static StepanCarService.AuthService.Tests.AuthTestUsers;

namespace StepanCarService.AuthService.Tests.Api;

// Отзыв токенов в Auth: security_stamp токена сверяется с текущим у пользователя
[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class SecurityStampTests(AuthServiceFactory factory)
    : ApiTestBase<AuthServiceFactory, Program, AuthDbContext>(factory)
{
    private const string ProtectedPath = "api/Auth/getTokenClaims";

    [Fact]
    public async Task TokenWithoutStampOrUserId_Returns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        using var client = Factory.CreateClientFor();

        (await client.WithBearer(TestTokenFactory.Create(Roles.TenantOwner, owner.Id)).GetAsync(ProtectedPath))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.WithBearer(TestTokenFactory.Create(Roles.TenantOwner, owner.Id, securityStamp: owner.SecurityStamp, includeUserId: false)).GetAsync(ProtectedPath))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await (await client.WithBearer(TokenFor(owner, Roles.TenantOwner)).GetAsync(ProtectedPath)).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StampChanged_OldTokenReturns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var token = TokenFor(owner, Roles.TenantOwner);
        await Factory.WithDbContextAsync(async db =>
        {
            (await db.Users.FindAsync(owner.Id))!.SecurityStamp = Guid.NewGuid().ToString("N");
            await db.SaveChangesAsync();
        });
        using var client = Factory.CreateClientFor().WithBearer(token);

        (await client.GetAsync(ProtectedPath)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Сравнение метки с учётом регистра
    [Fact]
    public async Task StampDifferingOnlyInCase_Returns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner).WithSecurityStamp("abcdef0123"));
        using var client = Factory.CreateClientFor()
            .WithBearer(TestTokenFactory.Create(Roles.TenantOwner, owner.Id, securityStamp: "ABCDEF0123"));

        (await client.GetAsync(ProtectedPath)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UserDeleted_TokenReturns401()
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var token = TokenFor(owner, Roles.TenantOwner);
        await Factory.WithDbContextAsync(async db =>
        {
            db.Users.Remove((await db.Users.FindAsync(owner.Id))!);
            await db.SaveChangesAsync();
        });
        using var client = Factory.CreateClientFor().WithBearer(token);

        (await client.GetAsync(ProtectedPath)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
