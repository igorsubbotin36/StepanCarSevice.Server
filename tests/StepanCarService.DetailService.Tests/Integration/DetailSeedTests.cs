using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Infrastructure.DBContexts.Inits;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.DetailService.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class DetailSeedTests(DetailDatabase database) : DatabaseTestBase<DetailDatabase>(database)
{
    // DT-65
    [Fact]
    public async Task Seed_CreatesDictionariesOnce()
    {
        await using (var again = Database.CreateContext())
            DbInitializer.Init(again);

        await using var db = Database.CreateContext();
        (await db.TransmissionTypes.CountAsync()).ShouldBe(4);
        (await db.EngineTypes.CountAsync()).ShouldBe(4);
        (await db.WheelDriveTypes.CountAsync()).ShouldBe(3);
    }
}
