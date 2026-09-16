using StepanCarService.Common.Core.Entities;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TenantService.Application.Services;
using StepanCarService.TestKit.Builders;

namespace StepanCarService.TenantService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class TenantRulesTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("a-b")]
    [InlineData("a1b2")]
    public void IsValidIdentifier_DnsLabel_ReturnsTrue(string identifier)
    {
        TenantManagementService.IsValidIdentifier(identifier).ShouldBeTrue();
    }

    [Theory]
    [InlineData("management")]
    [InlineData("www")]
    [InlineData("api")]
    public void IsValidIdentifier_Reserved_ReturnsFalse(string identifier)
    {
        TenantManagementService.IsValidIdentifier(identifier).ShouldBeFalse();
    }

    // Владелец не управляет чужим тенантом
    [Fact]
    public void CanManage_OwnerOfAnotherTenant_ReturnsFalse()
    {
        TenantInfoEntity tenant = TenantBuilder.Tenant().OwnedBy(8);

        TenantManagementService.CanManage(tenant, new TenantCaller(UserId: 7, IsGodMode: false)).ShouldBeFalse();
    }
}
