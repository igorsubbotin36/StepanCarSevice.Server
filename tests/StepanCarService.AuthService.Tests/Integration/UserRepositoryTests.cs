using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.AuthService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class UserRepositoryTests(AuthDatabase database) : DatabaseTestBase<AuthDatabase>(database)
{
    // AU-97
    [Fact]
    public async Task Seed_CreatesFourRoles()
    {
        await using var db = Database.CreateContext();

        (await db.Roles.Select(r => r.Name).OrderBy(n => n).ToListAsync())
            .ShouldBe(["GodMode", "TenantModerator", "TenantOwner", "User"]);
    }

    // AU-91
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
}
