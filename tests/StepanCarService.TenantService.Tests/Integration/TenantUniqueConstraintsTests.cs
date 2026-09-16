using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.TenantService.Tests.Integration;

// Уникальные индексы TenantServiceDbContext: Identifier — всегда, OwnerUserId — NULL не ограничен
[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class TenantUniqueConstraintsTests(TenantDatabase database) : DatabaseTestBase<TenantDatabase>(database)
{
    [Fact]
    public async Task DuplicateIdentifier_IsRejectedByDatabase()
    {
        var identifier = TestData.TenantIdentifier();
        await using (var db = Database.CreateContext())
        {
            db.Tenants.Add(TenantBuilder.Tenant().WithIdentifier(identifier));
            await db.SaveChangesAsync();
        }

        await using var duplicate = Database.CreateContext();
        duplicate.Tenants.Add(TenantBuilder.Tenant().WithIdentifier(identifier));
        await Should.ThrowAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicateOwnerUserId_IsRejectedByDatabase()
    {
        await using (var db = Database.CreateContext())
        {
            db.Tenants.Add(TenantBuilder.Tenant().OwnedBy(11));
            await db.SaveChangesAsync();
        }

        await using var duplicate = Database.CreateContext();
        duplicate.Tenants.Add(TenantBuilder.Tenant().OwnedBy(11));
        await Should.ThrowAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
    }

    // NULL в OwnerUserId (тенанты, созданные GodMode) не считаются одинаковыми — уникальный индекс их не ограничивает
    [Fact]
    public async Task MultipleTenantsWithoutOwner_AreAllowed()
    {
        await using var db = Database.CreateContext();
        db.Tenants.AddRange(TenantBuilder.Tenant().Build(), TenantBuilder.Tenant().Build());

        await Should.NotThrowAsync(() => db.SaveChangesAsync());
    }
}
