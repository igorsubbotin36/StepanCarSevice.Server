using Microsoft.EntityFrameworkCore;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;
using StepanCarService.TestKit.Conventions;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.VisitService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class TestConventionsTests
{
    [Fact]
    public void TestClasses_HaveOneCategoryMatchingFolder()
    {
        TestConventions.FindViolations(typeof(TestConventionsTests).Assembly).ShouldBeEmpty();
    }

    // CD-75, VS-30: модель строится без подключения к БД
    [Fact]
    public void VisitModel_AllTenantDataIsIsolated()
    {
        using var db = new VisitDBContext(new DbContextOptionsBuilder<VisitDBContext>().UseNpgsql("Host=unused").Options);

        TenantIsolationGuard.FindViolations(db).ShouldBeEmpty();
    }
}
