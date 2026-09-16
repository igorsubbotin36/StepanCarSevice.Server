using Microsoft.EntityFrameworkCore;
using StepanCarSevice.VisitService.Domain.Entities;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Exceptions;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.VisitService.Tests.Integration;

// Изоляция данных тенантов на реальных сущностях Visit (VS-30): структурную часть (ITenantScoped,
// фильтр запросов у каждой из 8 сущностей) проверяет TenantIsolationGuard в Unit/TestConventionsTests,
// здесь — фактическая работа фильтра и проверок записи на данных
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class VisitIsolationTests(VisitDatabase database) : DatabaseTestBase<VisitDatabase>(database)
{
    private readonly TenantInfoEntity _tenantA = TenantBuilder.Tenant().Build();
    private readonly TenantInfoEntity _tenantB = TenantBuilder.Tenant().Build();

    private VisitGraph _graphA = null!;
    private VisitGraph _graphB = null!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using (var db = Database.CreateContext())
        {
            db.Tenants.AddRange(_tenantA, _tenantB);
            await db.SaveChangesAsync();
        }
        _graphA = await SeedGraphAsync(_tenantA);
        _graphB = await SeedGraphAsync(_tenantB);
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

    [Fact]
    public async Task Queries_ReturnOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.Visits.CountAsync()).ShouldBe(1);
        (await db.Visits.ToListAsync()).ShouldAllBe(v => v.TenantId == _tenantA.Id);
        (await db.Visits.FindAsync(_graphB.Visit.Id)).ShouldBeNull();
        (await db.ManufactureSnapshots.CountAsync()).ShouldBe(1);
        (await db.CarModelSnapshots.CountAsync()).ShouldBe(1);
        (await db.OwnerSnapshots.CountAsync()).ShouldBe(1);
        (await db.CarSnapshots.CountAsync()).ShouldBe(1);
        (await db.Works.CountAsync()).ShouldBe(1);
        (await db.DetailSnapshots.CountAsync()).ShouldBe(1);
        (await db.VisitDetails.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Include_LoadsOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        var visits = await db.Visits.Include(v => v.Car).Include(v => v.Works).ToListAsync();

        visits.ShouldAllBe(v => v.TenantId == _tenantA.Id && v.Car.TenantId == _tenantA.Id);
        visits.SelectMany(v => v.Works).ShouldAllBe(w => w.TenantId == _tenantA.Id);
    }

    [Fact]
    public async Task ContextsOfDifferentTenants_EachSeeOwnData()
    {
        await using var dbA = Database.CreateContext(_tenantA);
        await using var dbB = Database.CreateContext(_tenantB);

        (await dbA.Visits.SingleAsync()).Id.ShouldBe(_graphA.Visit.Id);
        (await dbB.Visits.SingleAsync()).Id.ShouldBe(_graphB.Visit.Id);
        (await dbA.Visits.SingleAsync()).Id.ShouldBe(_graphA.Visit.Id);
    }

    [Fact]
    public async Task WithoutTenant_TenantDataIsHiddenButTenantsAreVisible()
    {
        await using var db = Database.CreateContext();

        (await db.Visits.AnyAsync()).ShouldBeFalse();
        (await db.CarSnapshots.AnyAsync()).ShouldBeFalse();
        (await db.OwnerSnapshots.AnyAsync()).ShouldBeFalse();
        (await db.Tenants.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Insert_WithoutTenantId_GetsCurrentTenant()
    {
        await using var db = Database.CreateContext(_tenantA);
        var owner = new OwnerSnapshot { FirstName = "Пётр", SecondName = "Новый", Phone = TestData.Phone() };
        db.OwnerSnapshots.Add(owner);

        await db.SaveChangesAsync();

        owner.TenantId.ShouldBe(_tenantA.Id);
    }

    [Fact]
    public async Task Insert_IntoOtherTenant_Throws()
    {
        await using var db = Database.CreateContext(_tenantA);
        db.OwnerSnapshots.Add(new OwnerSnapshot { FirstName = "Чужой", SecondName = "Владелец", Phone = TestData.Phone(), TenantId = _tenantB.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Insert_WithoutTenant_Throws()
    {
        await using var db = Database.CreateContext();
        db.OwnerSnapshots.Add(new OwnerSnapshot { FirstName = "С", SecondName = "Портала", Phone = TestData.Phone(), TenantId = _tenantA.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Update_DetachedEntityOfOtherTenant_ThrowsAndKeepsData()
    {
        OwnerSnapshot ownerB;
        await using (var loader = Database.CreateContext())
            ownerB = await loader.OwnerSnapshots.IgnoreQueryFilters().AsNoTracking().SingleAsync(o => o.Id == _graphB.Owner.Id);
        ownerB.FirstName = "Взломан";

        await using (var db = Database.CreateContext(_tenantA))
        {
            db.OwnerSnapshots.Update(ownerB);
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }

        await using var check = Database.CreateContext();
        (await check.OwnerSnapshots.IgnoreQueryFilters().SingleAsync(o => o.Id == _graphB.Owner.Id)).FirstName.ShouldBe(_graphB.Owner.FirstName);
    }

    [Fact]
    public async Task IgnoreQueryFilters_SeesAllTenants()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.Visits.IgnoreQueryFilters().Select(v => v.TenantId).Distinct().CountAsync()).ShouldBe(2);
    }
}
