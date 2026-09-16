using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.Conventions;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.DetailService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class TestConventionsTests
{
    [Fact]
    public void TestClasses_HaveOneCategoryMatchingFolder()
    {
        TestConventions.FindViolations(typeof(TestConventionsTests).Assembly).ShouldBeEmpty();
    }

    // Модель строится без подключения к БД
    [Fact]
    public void DetailModel_AllTenantDataIsIsolated()
    {
        using var db = new DetailDbContext(new DbContextOptionsBuilder<DetailDbContext>().UseNpgsql("Host=unused").Options);

        TenantIsolationGuard.FindViolations(db).ShouldBeEmpty();
    }

    // Данные Detail изолируются по тенанту общим контекстом
    [Fact]
    public void DetailDbContext_IsTenantScoped()
    {
        typeof(DetailDbContext).IsSubclassOf(typeof(TenantScopedDbContext)).ShouldBeTrue();
    }
}
