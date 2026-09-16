using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.Auth;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.Common.Tests.Unit;

// Правило политик *InTenant: роль из списка и tenant_id токена совпадает с тенантом запроса (GodMode — везде)
[Trait(TestCategories.Name, TestCategories.Unit)]
public class TenantRoleHandlerTests
{
    private static readonly TenantInfoEntity RequestTenant = TenantBuilder.Tenant().Build();
    private static readonly TenantRoleRequirement ModeratorRequirement = new(Roles.GodMode, Roles.TenantOwner, Roles.TenantModerator);

    private static async Task<bool> AuthorizeAsync(TenantInfoEntity? requestTenant, params Claim[] claims)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        var context = new AuthorizationHandlerContext([ModeratorRequirement], user, resource: null);

        await new TenantRoleHandler(new FakeTenantAccessor(requestTenant)).HandleAsync(context);
        return context.HasSucceeded;
    }

    private static Claim Role(string role) => new(ClaimTypes.Role, role);
    private static Claim TenantId(string tenantId) => new("tenant_id", tenantId);

    [Fact]
    public async Task RoleNotInRequirement_Fails()
    {
        (await AuthorizeAsync(RequestTenant, Role(Roles.User), TenantId(RequestTenant.Id!))).ShouldBeFalse();
    }

    [Fact]
    public async Task NoRoleClaim_Fails()
    {
        (await AuthorizeAsync(RequestTenant, TenantId(RequestTenant.Id!))).ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GodMode_SucceedsRegardlessOfTenant(bool inTenant)
    {
        (await AuthorizeAsync(inTenant ? RequestTenant : null, Role(Roles.GodMode))).ShouldBeTrue();
    }

    [Fact]
    public async Task MatchingRoleWithoutTenantId_Fails()
    {
        (await AuthorizeAsync(RequestTenant, Role(Roles.TenantModerator))).ShouldBeFalse();
    }

    [Fact]
    public async Task TenantIdOfAnotherTenant_Fails()
    {
        (await AuthorizeAsync(RequestTenant, Role(Roles.TenantModerator), TenantId(Guid.NewGuid().ToString()))).ShouldBeFalse();
    }

    [Fact]
    public async Task TenantIdOfRequestTenant_Succeeds()
    {
        (await AuthorizeAsync(RequestTenant, Role(Roles.TenantModerator), TenantId(RequestTenant.Id!))).ShouldBeTrue();
    }

    // Сравнение без учёта регистра (Q5 открыт); для GUID-идентификаторов это безопасно
    [Fact]
    public async Task TenantIdDifferingOnlyInCase_Succeeds()
    {
        (await AuthorizeAsync(RequestTenant, Role(Roles.TenantModerator), TenantId(RequestTenant.Id!.ToUpperInvariant()))).ShouldBeTrue();
    }

    [Fact]
    public async Task RequestWithoutTenant_Fails()
    {
        (await AuthorizeAsync(null, Role(Roles.TenantOwner), TenantId(RequestTenant.Id!))).ShouldBeFalse();
    }
}
