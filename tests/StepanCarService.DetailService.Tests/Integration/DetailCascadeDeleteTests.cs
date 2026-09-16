using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.DetailService.Tests.Integration;

// Каскадное удаление данных тенанта (DT-60, DT-61) и защита справочников/внутренних связей
// от удаления, пока на них есть ссылки (DT-62, DT-63)
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class DetailCascadeDeleteTests(DetailDatabase database) : DatabaseTestBase<DetailDatabase>(database)
{
    [Fact]
    public async Task DeletingTenant_CascadesFullGraph_OtherTenantUnaffected()
    {
        var tenantA = TenantBuilder.Tenant().Build();
        var tenantB = TenantBuilder.Tenant().Build();
        await SeedTenantsAsync(tenantA, tenantB);
        await SeedGraphAsync(tenantA);
        var graphB = await SeedGraphAsync(tenantB);

        await using (var db = Database.CreateContext())
        {
            db.Tenants.Remove(await db.Tenants.SingleAsync(t => t.Id == tenantA.Id));
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        (await check.Tenants.CountAsync(t => t.Id == tenantA.Id)).ShouldBe(0);
        (await check.CarManufactures.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.CarModels.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.CarModifications.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.DetailManufactures.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.Details.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.Engines.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);

        (await check.Tenants.CountAsync(t => t.Id == tenantB.Id)).ShouldBe(1);
        (await check.CarManufactures.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantB.Id)).Id.ShouldBe(graphB.Manufacture.Id);
        (await check.Details.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantB.Id)).Id.ShouldBe(graphB.Detail.Id);
    }

    [Fact]
    public async Task DeletingManufactureWithModels_IsRejectedByDatabase()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await SeedTenantsAsync(tenant);
        var graph = await SeedGraphAsync(tenant);

        await using var db = Database.CreateContext(tenant);
        db.CarManufactures.Remove(await db.CarManufactures.SingleAsync(m => m.Id == graph.Manufacture.Id));

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingUsedEngineType_IsRejectedByDatabase()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await SeedTenantsAsync(tenant);
        await SeedGraphAsync(tenant); // граф ссылается на EngineType Id = 1 из сида

        await using var db = Database.CreateContext();
        db.EngineTypes.Remove(await db.EngineTypes.SingleAsync(t => t.Id == 1));

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingUsedTransmissionType_IsRejectedByDatabase()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await SeedTenantsAsync(tenant);
        await SeedGraphAsync(tenant); // граф ссылается на TransmissionType Id = 1 из сида

        await using var db = Database.CreateContext();
        db.TransmissionTypes.Remove(await db.TransmissionTypes.SingleAsync(t => t.Id == 1));

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingUsedWheelDriveType_IsRejectedByDatabase()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await SeedTenantsAsync(tenant);
        await SeedGraphAsync(tenant); // граф ссылается на WheelDriveType Id = 1 из сида

        await using var db = Database.CreateContext();
        db.WheelDriveTypes.Remove(await db.WheelDriveTypes.SingleAsync(t => t.Id == 1));

        await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private async Task SeedTenantsAsync(params TenantInfoEntity[] tenants)
    {
        await using var db = Database.CreateContext();
        db.Tenants.AddRange(tenants);
        await db.SaveChangesAsync();
    }

    private async Task<DetailGraph> SeedGraphAsync(TenantInfoEntity tenant)
    {
        var graph = DetailGraphBuilder.Graph().Build();
        await using var db = Database.CreateContext(tenant);
        db.CarManufactures.Add(graph.Manufacture);
        db.DetailManufactures.Add(graph.DetailManufacture);
        await db.SaveChangesAsync();
        return graph;
    }
}
