using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Infrastructure.Repositories;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Exceptions;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.DetailService.Tests.Integration;

// Изоляция данных тенантов на реальных сущностях Detail: CD-60…76 из общего плана, DT-68
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class DetailIsolationTests(DetailDatabase database) : DatabaseTestBase<DetailDatabase>(database)
{
    private readonly TenantInfoEntity _tenantA = TenantBuilder.Tenant().Build();
    private readonly TenantInfoEntity _tenantB = TenantBuilder.Tenant().Build();

    private DetailGraph _graphA = null!;
    private DetailGraph _graphB = null!;

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

    private async Task<DetailGraph> SeedGraphAsync(TenantInfoEntity tenant)
    {
        var graph = DetailGraphBuilder.Graph().Build();
        await using var db = Database.CreateContext(tenant);
        db.CarManufactures.Add(graph.Manufacture);
        db.DetailManufactures.Add(graph.DetailManufacture);
        await db.SaveChangesAsync();
        return graph;
    }

    [Fact]
    public async Task Queries_ReturnOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.CarManufactures.CountAsync()).ShouldBe(1);
        (await db.CarManufactures.ToListAsync()).ShouldAllBe(m => m.TenantId == _tenantA.Id);
        (await db.CarManufactures.FindAsync(_graphB.Manufacture.Id)).ShouldBeNull();
        (await db.Details.CountAsync()).ShouldBe(1);
        (await db.Engines.CountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Include_LoadsOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        var manufactures = await db.CarManufactures.Include(m => m.CarModels).ThenInclude(model => model.CarModifications).ToListAsync();

        manufactures.ShouldAllBe(m => m.TenantId == _tenantA.Id);
        manufactures.SelectMany(m => m.CarModels).ShouldAllBe(m => m.TenantId == _tenantA.Id);
        manufactures.SelectMany(m => m.CarModels).SelectMany(m => m.CarModifications).ShouldAllBe(m => m.TenantId == _tenantA.Id);
    }

    [Fact]
    public async Task ContextsOfDifferentTenants_EachSeeOwnData()
    {
        await using var dbA = Database.CreateContext(_tenantA);
        await using var dbB = Database.CreateContext(_tenantB);

        (await dbA.CarManufactures.SingleAsync()).Id.ShouldBe(_graphA.Manufacture.Id);
        (await dbB.CarManufactures.SingleAsync()).Id.ShouldBe(_graphB.Manufacture.Id);
        (await dbA.CarManufactures.SingleAsync()).Id.ShouldBe(_graphA.Manufacture.Id);
    }

    [Fact]
    public async Task WithoutTenant_TenantDataIsHiddenButDictionariesAreVisible()
    {
        await using var db = Database.CreateContext();

        (await db.CarManufactures.AnyAsync()).ShouldBeFalse();
        (await db.Details.AnyAsync()).ShouldBeFalse();
        (await db.EngineTypes.CountAsync()).ShouldBe(4);
        (await db.TransmissionTypes.CountAsync()).ShouldBe(4);
        (await db.WheelDriveTypes.CountAsync()).ShouldBe(3);
        (await db.Tenants.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Insert_WithoutTenantId_GetsCurrentTenant()
    {
        await using var db = Database.CreateContext(_tenantA);
        var manufacture = new CarManufacture { Name = "Новый" };
        db.CarManufactures.Add(manufacture);

        await db.SaveChangesAsync();

        manufacture.TenantId.ShouldBe(_tenantA.Id);
    }

    [Fact]
    public async Task Insert_IntoOtherTenant_Throws()
    {
        await using var db = Database.CreateContext(_tenantA);
        db.CarManufactures.Add(new CarManufacture { Name = "Чужой", TenantId = _tenantB.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Insert_WithoutTenant_Throws()
    {
        await using var db = Database.CreateContext();
        db.CarManufactures.Add(new CarManufacture { Name = "С портала", TenantId = _tenantA.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Update_DetachedEntityOfOtherTenant_ThrowsAndKeepsData()
    {
        CarManufacture manufactureB;
        await using (var loader = Database.CreateContext())
            manufactureB = await loader.CarManufactures.IgnoreQueryFilters().AsNoTracking().SingleAsync(m => m.Id == _graphB.Manufacture.Id);
        manufactureB.Name = "Взломан";

        await using (var db = Database.CreateContext(_tenantA))
        {
            db.CarManufactures.Update(manufactureB);
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }

        await using var check = Database.CreateContext();
        (await check.CarManufactures.IgnoreQueryFilters().SingleAsync(m => m.Id == _graphB.Manufacture.Id)).Name.ShouldBe(_graphB.Manufacture.Name);
    }

    [Fact]
    public async Task Delete_DetachedEntityOfOtherTenant_Throws()
    {
        await using (var db = Database.CreateContext(_tenantA))
        {
            db.CarManufactures.Remove(new CarManufacture { Id = _graphB.Manufacture.Id, Name = "x", TenantId = _tenantB.Id! });
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }

        await using var check = Database.CreateContext();
        (await check.CarManufactures.IgnoreQueryFilters().AnyAsync(m => m.Id == _graphB.Manufacture.Id)).ShouldBeTrue();
    }

    [Fact]
    public async Task IgnoreQueryFilters_SeesAllTenants()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.CarManufactures.IgnoreQueryFilters().Select(m => m.TenantId).Distinct().CountAsync()).ShouldBe(2);
    }

    // DT-68: поиск через общий справочный репозиторий тоже фильтруется по тенанту (Engine наследует AppCommonEntity)
    [Fact]
    public async Task CommonRepository_Engine_IsFilteredByTenant()
    {
        await using var db = Database.CreateContext(_tenantA);
        var repository = new CommonRepository<Engine>(db);

        var all = await repository.GetAllAsync();
        var byName = await repository.GetByNameAsync(_graphA.Engine.Name);
        var otherTenantEngine = await repository.GetByIdAsync(_graphB.Engine.Id);

        all.ShouldHaveSingleItem().Id.ShouldBe(_graphA.Engine.Id);
        byName.ShouldHaveSingleItem().Id.ShouldBe(_graphA.Engine.Id);
        otherTenantEngine.ShouldBeNull();
    }
}
