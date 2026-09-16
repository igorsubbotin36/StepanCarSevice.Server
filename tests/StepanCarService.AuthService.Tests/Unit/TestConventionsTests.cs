using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.Conventions;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarService.AuthService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class TestConventionsTests
{
    [Fact]
    public void TestClasses_HaveOneCategoryMatchingFolder()
    {
        TestConventions.FindViolations(typeof(TestConventionsTests).Assembly).ShouldBeEmpty();
    }

    // Пользователи Auth — не данные тенанта (у владельцев и GodMode TenantId = null), фильтр тенантов к ним не применяется
    [Fact]
    public void AuthDbContext_IsNotTenantScoped()
    {
        typeof(AuthDbContext).IsSubclassOf(typeof(TenantScopedDbContext)).ShouldBeFalse();
    }
}
