using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Exceptions;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.Common.Tests.Integration;

// Ядро изоляции данных тенантов: фильтр запросов и проверка записи в TenantScopedDbContext
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class TenantScopedDbContextTests(ScopedTestDatabase database) : DatabaseTestBase<ScopedTestDatabase>(database)
{
    private readonly TenantInfoEntity _tenantA = TenantBuilder.Tenant().Build();
    private readonly TenantInfoEntity _tenantB = TenantBuilder.Tenant().Build();

    private int _garageA;
    private int _garageB;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var db = Database.CreateContext();
        db.Tenants.AddRange(_tenantA, _tenantB);
        await db.SaveChangesAsync();

        _garageA = await AddGarageAsync(_tenantA, "Гараж A");
        _garageB = await AddGarageAsync(_tenantB, "Гараж B");
    }

    private async Task<int> AddGarageAsync(TenantInfoEntity tenant, string name)
    {
        await using var db = Database.CreateContext(tenant);
        var category = new Category { Name = $"Категория {name}" };
        var garage = new Garage { Name = name, Category = category, Cars = [new Car { Model = $"{name} машина 1" }, new Car { Model = $"{name} машина 2" }] };
        db.Garages.Add(garage);
        await db.SaveChangesAsync();
        return garage.Id;
    }

    private async Task<Garage> LoadAsIgnoringFiltersAsync(int id)
    {
        await using var db = Database.CreateContext();
        return await db.Garages.IgnoreQueryFilters().AsNoTracking().SingleAsync(g => g.Id == id);
    }

    [Fact]
    public async Task Queries_ReturnOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.Garages.CountAsync()).ShouldBe(1);
        (await db.Garages.ToListAsync()).ShouldAllBe(g => g.TenantId == _tenantA.Id);
        (await db.Garages.FindAsync(_garageB)).ShouldBeNull();
        (await db.Garages.FirstOrDefaultAsync(g => g.Id == _garageB)).ShouldBeNull();
        (await db.Cars.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Include_LoadsOnlyCurrentTenantRows()
    {
        await using var db = Database.CreateContext(_tenantA);

        var cars = await db.Cars.Include(c => c.Garage).ToListAsync();
        var garages = await db.Garages.Include(g => g.Cars).Include(g => g.Category).ToListAsync();

        cars.ShouldAllBe(c => c.TenantId == _tenantA.Id && c.Garage!.TenantId == _tenantA.Id);
        garages.SelectMany(g => g.Cars).ShouldAllBe(c => c.TenantId == _tenantA.Id);
    }

    [Fact]
    public async Task Projection_IsFiltered()
    {
        await using var db = Database.CreateContext(_tenantA);

        var names = await db.Garages.Select(g => new { g.Name, CarCount = g.Cars.Count }).ToListAsync();

        names.ShouldHaveSingleItem().Name.ShouldBe("Гараж A");
    }

    [Fact]
    public async Task FromSqlRaw_IsFiltered()
    {
        await using var db = Database.CreateContext(_tenantA);

        var garages = await db.Garages.FromSqlRaw("SELECT * FROM \"Garages\"").Where(g => g.Name != "").ToListAsync();

        garages.ShouldHaveSingleItem().Id.ShouldBe(_garageA);
    }

    [Fact]
    public async Task ExecuteUpdateAndDelete_DoNotTouchOtherTenantRows()
    {
        await using (var db = Database.CreateContext(_tenantA))
        {
            (await db.Garages.Where(g => g.Id == _garageB).ExecuteUpdateAsync(s => s.SetProperty(g => g.Name, "Взломан"))).ShouldBe(0);
            (await db.Cars.Where(c => c.GarageId == _garageB).ExecuteDeleteAsync()).ShouldBe(0);
        }

        (await LoadAsIgnoringFiltersAsync(_garageB)).Name.ShouldBe("Гараж B");
        await using var check = Database.CreateContext(_tenantB);
        (await check.Cars.CountAsync()).ShouldBe(2);
    }

    // Модель EF кэшируется на тип контекста, фильтр должен брать тенант каждого экземпляра
    [Fact]
    public async Task ContextsOfDifferentTenants_EachSeeOwnData()
    {
        await using var dbA = Database.CreateContext(_tenantA);
        await using var dbB = Database.CreateContext(_tenantB);

        (await dbA.Garages.SingleAsync()).Id.ShouldBe(_garageA);
        (await dbB.Garages.SingleAsync()).Id.ShouldBe(_garageB);
        (await dbA.Garages.SingleAsync()).Id.ShouldBe(_garageA);
    }

    [Fact]
    public async Task WithoutTenant_TenantDataIsHiddenButDictionariesAndTenantsAreVisible()
    {
        await using var db = Database.CreateContext();

        (await db.Garages.AnyAsync()).ShouldBeFalse();
        (await db.Cars.AnyAsync()).ShouldBeFalse();
        (await db.Categories.CountAsync()).ShouldBe(2);
        (await db.Tenants.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Insert_WithoutTenantId_GetsCurrentTenant()
    {
        await using var db = Database.CreateContext(_tenantA);
        var garage = new Garage { Name = "Новый" };
        db.Garages.Add(garage);

        await db.SaveChangesAsync();

        (await LoadAsIgnoringFiltersAsync(garage.Id)).TenantId.ShouldBe(_tenantA.Id);
    }

    [Fact]
    public async Task Insert_IntoOtherTenant_Throws()
    {
        await using var db = Database.CreateContext(_tenantA);
        db.Garages.Add(new Garage { Name = "Чужой", TenantId = _tenantB.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Insert_WithoutTenant_Throws()
    {
        await using var db = Database.CreateContext();
        db.Garages.Add(new Garage { Name = "С портала", TenantId = _tenantA.Id! });

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Update_DetachedEntityOfOtherTenant_ThrowsAndKeepsData()
    {
        var garageB = await LoadAsIgnoringFiltersAsync(_garageB);
        garageB.Name = "Взломан";

        await using (var db = Database.CreateContext(_tenantA))
        {
            db.Garages.Update(garageB);
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }
        await using (var db = Database.CreateContext(_tenantA))
        {
            db.Garages.Attach(garageB).Property(g => g.Name).IsModified = true;
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }

        (await LoadAsIgnoringFiltersAsync(_garageB)).Name.ShouldBe("Гараж B");
    }

    [Fact]
    public async Task Delete_DetachedEntityOfOtherTenant_Throws()
    {
        await using (var db = Database.CreateContext(_tenantA))
        {
            db.Cars.Remove(new Car { Id = 1, Model = "x", TenantId = _tenantB.Id!, GarageId = _garageB });
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }
        await using (var db = Database.CreateContext(_tenantA))
        {
            db.Garages.Remove(await LoadAsIgnoringFiltersAsync(_garageB));
            await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
        }

        (await LoadAsIgnoringFiltersAsync(_garageB)).ShouldNotBeNull();
    }

    [Fact]
    public async Task MovingOwnEntityToOtherTenant_Throws()
    {
        await using var db = Database.CreateContext(_tenantA);
        var garage = await db.Garages.SingleAsync();
        garage.TenantId = _tenantB.Id!;

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    // Смена TenantId в обратную сторону — «перетащить» чужую строку к себе
    [Fact]
    public async Task ClaimingEntityOfOtherTenant_Throws()
    {
        var garageB = await LoadAsIgnoringFiltersAsync(_garageB);
        await using var db = Database.CreateContext(_tenantA);
        db.Garages.Attach(garageB);
        garageB.TenantId = _tenantA.Id!;

        await Should.ThrowAsync<TenantAccessViolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void SyncSaveChanges_AppliesSameRules()
    {
        using (var db = Database.CreateContext(_tenantA))
        {
            db.Garages.Add(new Garage { Name = "Чужой", TenantId = _tenantB.Id! });
            Should.Throw<TenantAccessViolationException>(() => db.SaveChanges());
        }
        using (var db = Database.CreateContext())
        {
            db.Garages.Add(new Garage { Name = "С портала" });
            Should.Throw<TenantAccessViolationException>(() => db.SaveChanges());
        }
        using (var db = Database.CreateContext(_tenantA))
        {
            var garage = new Garage { Name = "Свой" };
            db.Garages.Add(garage);
            db.SaveChanges();
            garage.TenantId.ShouldBe(_tenantA.Id);
        }
    }

    // Явный обход фильтра (отчёты GodMode) видит все тенанты
    [Fact]
    public async Task IgnoreQueryFilters_SeesAllTenants()
    {
        await using var db = Database.CreateContext(_tenantA);

        (await db.Garages.IgnoreQueryFilters().Select(g => g.TenantId).Distinct().CountAsync()).ShouldBe(2);
    }
}
