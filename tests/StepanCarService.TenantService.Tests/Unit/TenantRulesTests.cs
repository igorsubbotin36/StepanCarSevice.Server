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

    [Theory]
    [InlineData("ab")]                  // короче 3 символов
    [InlineData("Abc")]                 // заглавные буквы
    [InlineData("-abc")]                // начинается с дефиса
    [InlineData("abc-")]                // заканчивается дефисом
    [InlineData("a_b")]                 // подчёркивание
    [InlineData("a b")]                 // пробел
    [InlineData("тенант")]              // кириллица
    [InlineData("")]                    // пусто
    [InlineData(null)]                  // null
    public void IsValidIdentifier_InvalidDnsLabel_ReturnsFalse(string? identifier)
    {
        TenantManagementService.IsValidIdentifier(identifier).ShouldBeFalse();
    }

    [Fact]
    public void IsValidIdentifier_ExactBoundaryLengths_ReturnsExpected()
    {
        TenantManagementService.IsValidIdentifier(new string('a', 63)).ShouldBeTrue();
        TenantManagementService.IsValidIdentifier(new string('a', 64)).ShouldBeFalse();
    }

    // Владелец не управляет чужим тенантом
    [Fact]
    public void CanManage_OwnerOfAnotherTenant_ReturnsFalse()
    {
        TenantInfoEntity tenant = TenantBuilder.Tenant().OwnedBy(8);

        TenantManagementService.CanManage(tenant, new TenantCaller(UserId: 7, IsGodMode: false)).ShouldBeFalse();
    }

    [Fact]
    public void CanManage_GodMode_ReturnsTrueRegardlessOfOwner()
    {
        TenantInfoEntity tenant = TenantBuilder.Tenant().OwnedBy(8);

        TenantManagementService.CanManage(tenant, new TenantCaller(UserId: null, IsGodMode: true)).ShouldBeTrue();
    }

    [Fact]
    public void CanManage_OwnerOfThisTenant_ReturnsTrue()
    {
        TenantInfoEntity tenant = TenantBuilder.Tenant().OwnedBy(7);

        TenantManagementService.CanManage(tenant, new TenantCaller(UserId: 7, IsGodMode: false)).ShouldBeTrue();
    }

    // Аноним (нет claim пользователя) не управляет даже тенантом без владельца
    [Fact]
    public void CanManage_CallerWithoutUserId_ReturnsFalse()
    {
        TenantInfoEntity tenant = TenantBuilder.Tenant().Build();

        TenantManagementService.CanManage(tenant, new TenantCaller(UserId: null, IsGodMode: false)).ShouldBeFalse();
    }
}
