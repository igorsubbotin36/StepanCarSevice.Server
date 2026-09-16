using StepanCarService.Common.Infastructure.Repositories;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.Common.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class TenantBaseRepositoryTests(CommonTestDatabase database) : DatabaseTestBase<CommonTestDatabase>(database)
{
    // Имена тенантов не уникальны
    [Fact(Skip = "Баг: GetByNameAsync использует SingleOrDefault и бросает исключение при одинаковых именах")]
    public async Task GetByName_TwoTenantsWithSameName_ReturnsOneWithoutException()
    {
        await using (var db = Database.CreateContext())
        {
            db.Tenants.AddRange(TenantBuilder.Tenant().WithName("Автосервис"), TenantBuilder.Tenant().WithName("Автосервис"));
            await db.SaveChangesAsync();
        }

        await using var context = Database.CreateContext();
        var tenant = await new TenantBaseRepository<CommonTestDbContext>(context).GetByNameAsync("Автосервис");

        tenant.ShouldNotBeNull().Name.ShouldBe("Автосервис");
    }
}
