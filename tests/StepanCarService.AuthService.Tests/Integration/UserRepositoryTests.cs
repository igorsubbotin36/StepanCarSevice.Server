using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Infrastructure.Repositories;

namespace StepanCarService.AuthService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class UserRepositoryTests(AuthDatabase database) : DatabaseTestBase<AuthDatabase>(database)
{
    // Роли сида совпадают с константами Roles, на которые опираются политики
    [Fact]
    public async Task Seed_CreatesFourRolesMatchingRoleConstants()
    {
        await using var db = Database.CreateContext();

        (await db.Roles.Select(r => r.Name).OrderBy(n => n).ToListAsync())
            .ShouldBe(new[] { Roles.GodMode, Roles.TenantModerator, Roles.TenantOwner, Roles.User }.Order());
        (await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id)).ShouldBe(new Dictionary<string, int>
        {
            [Roles.GodMode] = RoleIds.GodMode,
            [Roles.User] = RoleIds.User,
            [Roles.TenantOwner] = RoleIds.TenantOwner,
            [Roles.TenantModerator] = RoleIds.TenantModerator
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task UniquePhoneIndex_RejectsDuplicatePhoneOnPortal()
    {
        var phone = TestData.Phone();
        await using (var db = Database.CreateContext())
        {
            db.Users.Add(UserBuilder.User().WithRole(RoleIds.TenantOwner).WithPhone(phone));
            await db.SaveChangesAsync();
        }

        await using var second = Database.CreateContext();
        second.Users.Add(UserBuilder.User().WithRole(RoleIds.TenantOwner).WithPhone(phone));

        await Should.ThrowAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    // В пределах одного тенанта тоже, а в разных тенантах телефон может повторяться
    [Fact]
    public async Task UniquePhoneIndex_IsPerTenant()
    {
        var tenantA = await Database.SeedTenantAsync(TenantBuilder.Tenant());
        var tenantB = await Database.SeedTenantAsync(TenantBuilder.Tenant());
        var phone = TestData.Phone();
        await using (var db = Database.CreateContext())
        {
            db.Users.AddRange(
                UserBuilder.User().InTenant(tenantA.Id).WithPhone(phone),
                UserBuilder.User().InTenant(tenantB.Id).WithPhone(phone),
                UserBuilder.User().WithRole(RoleIds.TenantOwner).WithPhone(phone));
            await db.SaveChangesAsync();
        }

        await using var duplicate = Database.CreateContext();
        duplicate.Users.Add(UserBuilder.User().InTenant(tenantA.Id).WithPhone(phone));
        await Should.ThrowAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
    }

    // tenantId = null ищет пользователей портала (IS NULL), а не всех
    [Fact]
    public async Task Queries_WithNullTenant_FindOnlyPortalUsers()
    {
        var tenant = await Database.SeedTenantAsync(TenantBuilder.Tenant());
        var phone = TestData.Phone();
        User owner = UserBuilder.User().WithRole(RoleIds.TenantOwner).WithPhone(phone);
        User tenantUser = UserBuilder.User().InTenant(tenant.Id).WithPhone(phone);
        await using (var db = Database.CreateContext())
        {
            db.Users.AddRange(owner, tenantUser);
            await db.SaveChangesAsync();
        }

        await using var context = Database.CreateContext();
        var repository = new UserRepository(context);

        (await repository.GetUserByPhoneAsync(phone, null)).ShouldNotBeNull().Id.ShouldBe(owner.Id);
        (await repository.GetUserByPhoneAsync(phone, tenant.Id)).ShouldNotBeNull().Id.ShouldBe(tenantUser.Id);
        (await repository.ExistsByPhoneAsync(phone, null)).ShouldBeTrue();
        (await repository.GetAllUsersAsync(null)).Select(u => u.Id).ShouldBe([owner.Id]);
        (await repository.GetAllUsersAsync(tenant.Id)).Select(u => u.Id).ShouldBe([tenantUser.Id]);
        (await repository.GetUserByIdAsync(tenantUser.Id, null)).ShouldBeNull();
        (await repository.GetSecurityStampAsync(owner.Id)).ShouldBe(owner.SecurityStamp);
    }

    // Пользователи тенанта удаляются вместе с ним, пользователи портала (в том числе владелец) остаются
    [Fact]
    public async Task DeletingTenant_CascadesToItsUsersOnly()
    {
        User owner = UserBuilder.User().WithRole(RoleIds.TenantOwner);
        await using (var db = Database.CreateContext())
        {
            db.Users.Add(owner);
            await db.SaveChangesAsync();
        }
        var tenant = await Database.SeedTenantAsync(TenantBuilder.Tenant().OwnedBy(owner.Id));
        var otherTenant = await Database.SeedTenantAsync(TenantBuilder.Tenant());
        await using (var db = Database.CreateContext())
        {
            db.Users.AddRange(UserBuilder.User().InTenant(tenant.Id), UserBuilder.User().InTenant(tenant.Id).WithRole(RoleIds.TenantModerator),
                UserBuilder.User().InTenant(otherTenant.Id));
            await db.SaveChangesAsync();
        }

        await using (var db = Database.CreateContext())
        {
            db.Tenants.Remove((await db.Tenants.FindAsync(tenant.Id))!);
            await db.SaveChangesAsync();
        }

        await using var check = Database.CreateContext();
        (await check.Users.CountAsync(u => u.TenantId == tenant.Id)).ShouldBe(0);
        (await check.Users.CountAsync(u => u.TenantId == otherTenant.Id)).ShouldBe(1);
        (await check.Users.AnyAsync(u => u.Id == owner.Id)).ShouldBeTrue();
    }

    [Fact(Skip = "Баг: FK Users.RoleId с ON DELETE CASCADE — удаление роли удаляет всех её пользователей")]
    public async Task DeletingRoleInUse_IsRejectedAndKeepsUsers()
    {
        await using (var db = Database.CreateContext())
        {
            db.Users.Add(UserBuilder.User().WithRole(RoleIds.TenantOwner));
            await db.SaveChangesAsync();
        }

        await using (var db = Database.CreateContext())
        {
            db.Roles.Remove((await db.Roles.FindAsync(RoleIds.TenantOwner))!);
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        await using var check = Database.CreateContext();
        (await check.Users.CountAsync(u => u.RoleId == RoleIds.TenantOwner)).ShouldBe(1);
    }

    [Fact(Skip = "Баг: сид ролей с явными Id не сдвигает последовательность — новая роль получает Id = 1 и конфликтует")]
    public async Task AddingRoleAfterSeed_GetsFreeId()
    {
        await using var db = Database.CreateContext();
        var role = new Role { Name = "Accountant", Description = "Бухгалтер" };
        db.Roles.Add(role);

        await Should.NotThrowAsync(() => db.SaveChangesAsync());
        role.Id.ShouldBeGreaterThan(RoleIds.TenantModerator);
    }
}
