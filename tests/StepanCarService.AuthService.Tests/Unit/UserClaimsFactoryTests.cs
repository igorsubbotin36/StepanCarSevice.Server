using System.Security.Claims;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Builders;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Domain.Entities;

namespace StepanCarService.AuthService.Tests.Unit;

// Claims токена, который выдаёт Auth: от них зависят проверки во всех сервисах
[Trait(TestCategories.Name, TestCategories.Unit)]
public class UserClaimsFactoryTests
{
    private static User UserWithRole(UserBuilder builder, string role)
    {
        var user = builder.Build();
        user.Id = 42;
        user.Role = new Role { Name = role, Description = role };
        return user;
    }

    [Fact]
    public void TenantUser_HasAllIdentityClaimsAndTenantId()
    {
        var tenantId = Guid.NewGuid().ToString();
        var user = UserWithRole(UserBuilder.User().InTenant(tenantId).WithPhone("+79990000001").WithEmail("user@example.com")
            .WithName("Иван", "Петров").WithSecurityStamp("stamp-1"), Roles.User);

        var identity = UserClaimsFactory.BuildIdentity(user);

        identity.FindFirst(ClaimTypes.NameIdentifier)!.Value.ShouldBe("42");
        identity.FindFirst(ClaimTypes.MobilePhone)!.Value.ShouldBe("+79990000001");
        identity.FindFirst(ClaimTypes.Role)!.Value.ShouldBe(Roles.User);
        identity.FindFirst(ClaimTypes.Email)!.Value.ShouldBe("user@example.com");
        identity.FindFirst(ClaimTypes.GivenName)!.Value.ShouldBe("Иван");
        identity.FindFirst(ClaimTypes.Surname)!.Value.ShouldBe("Петров");
        identity.FindFirst(AuthClaimTypes.SecurityStamp)!.Value.ShouldBe("stamp-1");
        identity.FindFirst(AuthClaimTypes.TenantId)!.Value.ShouldBe(tenantId);
        identity.RoleClaimType.ShouldBe(ClaimTypes.Role);
    }

    // Права владельца в тенанте проверяются при каждом запросе, а не зашиваются в токен
    [Theory]
    [InlineData(Roles.TenantOwner)]
    [InlineData(Roles.GodMode)]
    public void PortalUser_HasNoTenantId(string role)
    {
        var identity = UserClaimsFactory.BuildIdentity(UserWithRole(UserBuilder.User(), role));

        identity.FindFirst(AuthClaimTypes.TenantId).ShouldBeNull();
        identity.FindFirst(ClaimTypes.Role)!.Value.ShouldBe(role);
    }

    [Fact]
    public void MissingOptionalFields_BecomeEmptyStrings()
    {
        var user = UserWithRole(UserBuilder.User().WithName(null, null), Roles.TenantOwner);
        user.Phone = null;

        var identity = UserClaimsFactory.BuildIdentity(user);

        identity.FindFirst(ClaimTypes.MobilePhone)!.Value.ShouldBeEmpty();
        identity.FindFirst(ClaimTypes.GivenName)!.Value.ShouldBeEmpty();
        identity.FindFirst(ClaimTypes.Surname)!.Value.ShouldBeEmpty();
    }
}
