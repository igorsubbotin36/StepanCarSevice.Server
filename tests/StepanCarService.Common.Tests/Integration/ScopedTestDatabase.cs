using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.Common.Tests.Integration;

// Данные тенанта: гараж с машинами (для Include) и ссылкой на общий справочник
public class Garage : ITenantScoped
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public required string Name { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Car> Cars { get; set; } = [];
}

public class Car : ITenantScoped
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public required string Model { get; set; }
    public int GarageId { get; set; }
    public Garage? Garage { get; set; }
}

// Справочник, общий для всех тенантов
public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public sealed class ScopedTestDbContext(DbContextOptions<ScopedTestDbContext> options, IMultiTenantContextAccessor<TenantInfoEntity>? tenantAccessor)
    : TenantScopedDbContext(options, tenantAccessor)
{
    public DbSet<Garage> Garages => Set<Garage>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Garage>().HasMany(g => g.Cars).WithOne(c => c.Garage).HasForeignKey(c => c.GarageId);
        modelBuilder.Entity<Garage>().HasOne(g => g.Category).WithMany().HasForeignKey(g => g.CategoryId);
    }
}

// БД с тестовым контекстом на TenantScopedDbContext: проверка изоляции без привязки к модели Detail/Visit
public sealed class ScopedTestDatabase : PostgresDatabase
{
    protected override string Prefix => "scoped";

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    // Контекст в тенанте tenant; null — портал, фоновая задача
    public ScopedTestDbContext CreateContext(TenantInfoEntity? tenant = null) =>
        new(new DbContextOptionsBuilder<ScopedTestDbContext>().UseNpgsql(ConnectionString).Options, new FakeTenantAccessor(tenant));
}
