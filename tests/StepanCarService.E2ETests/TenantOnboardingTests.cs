namespace StepanCarService.E2ETests;

[Trait(TestCategories.Name, TestCategories.E2E)]
[Collection(PlatformCollection.Name)]
public class TenantOnboardingTests(PlatformFixture platform)
{
    // Владелец создаёт тенант, тенант реплицируется во все сервисы, владелец входит на свой поддомен
    [Fact]
    public async Task OwnerCreatesTenant_TenantReplicatedAndOwnerLogsInOnSubdomain()
    {
        var (tenant, phone, password, _) = await platform.OnboardTenantAsync();
        tenant.OwnerUserId.ShouldNotBeNull();

        using var subdomain = platform.Auth.CreateClientFor(tenant.Identifier);
        await subdomain.LoginAsync(phone, password);
    }
}
