using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.VisitService.Tests.Integration;

// Каскадное удаление тенанта уносит весь его граф (машины, владельцы, модели, визиты, работы,
// детали визитов), другой тенант не затронут (VS-31)
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class VisitCascadeDeleteTests(VisitDatabase database) : DatabaseTestBase<VisitDatabase>(database)
{
    [Fact]
    public async Task DeletingTenant_CascadesFullGraph_OtherTenantUnaffected()
    {
        var tenantA = TenantBuilder.Tenant().Build();
        var tenantB = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            db.Tenants.AddRange(tenantA, tenantB);
            await db.SaveChangesAsync();
        }

        await SeedGraphAsync(tenantA);
        var graphB = await SeedGraphAsync(tenantB);

        await using (var db = Database.CreateContext())
        {
            db.Tenants.Remove(await db.Tenants.SingleAsync(t => t.Id == tenantA.Id));
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        (await check.Tenants.CountAsync(t => t.Id == tenantA.Id)).ShouldBe(0);
        (await check.ManufactureSnapshots.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.CarModelSnapshots.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.OwnerSnapshots.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.CarSnapshots.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.Visits.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.Works.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.DetailSnapshots.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);
        (await check.VisitDetails.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantA.Id)).ShouldBe(0);

        (await check.Tenants.CountAsync(t => t.Id == tenantB.Id)).ShouldBe(1);
        (await check.Visits.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantB.Id)).Id.ShouldBe(graphB.Visit.Id);
        (await check.Works.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantB.Id)).Id.ShouldBe(graphB.Work.Id);
        (await check.VisitDetails.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantB.Id)).Id.ShouldBe(graphB.VisitDetails.Id);
    }

    private async Task<VisitGraph> SeedGraphAsync(TenantInfoEntity tenant)
    {
        var graph = VisitGraphBuilder.Graph().Build();
        await using var db = Database.CreateContext(tenant);
        db.Visits.Add(graph.Visit);
        db.VisitDetails.Add(graph.VisitDetails);
        await db.SaveChangesAsync();
        return graph;
    }
}
