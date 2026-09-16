using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Infastructure.Repositories;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.Common.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class UnitsOfWorkTests(CommonTestDatabase database) : DatabaseTestBase<CommonTestDatabase>(database)
{
    // CD-42
    [Fact]
    public async Task Commit_SavesChanges()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(tenant);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
        }

        await using var check = Database.CreateContext();
        (await check.Tenants.AnyAsync(t => t.Id == tenant.Id)).ShouldBeTrue();
    }

    // CD-43
    [Fact]
    public async Task Rollback_DiscardsSavedChanges()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(tenant);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.RollBackTransactionAsync();
        }

        await using var check = Database.CreateContext();
        (await check.Tenants.AnyAsync(t => t.Id == tenant.Id)).ShouldBeFalse();
    }
}
