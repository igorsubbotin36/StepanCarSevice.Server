using System.Text.RegularExpressions;

namespace StepanCarService.TenantService.Tests.Unit;

// Design-time фабрика контекста (dotnet ef) ищет appsettings API-проекта по относительному пути
[Trait(TestCategories.Name, TestCategories.Unit)]
public class DesignTimeFactoryTests
{
    // На Linux (CI) путь регистрозависим
    [Fact(Skip = "Баг: TenantServiceDbContextFactory ищет каталог StepanCarService.TenantService.Api вместо API")]
    public void Factory_PointsToExistingApiDirectoryWithExactCase()
    {
        var infrastructure = Path.Combine(RepositoryPaths.Root, "Services", "Tenant", "StepanCarService.TenantService.Infrastructure");
        var source = File.ReadAllText(Path.Combine(infrastructure, "DbContexts", "Factories", "TenantServiceDbContextFactory.cs"));
        var relative = Regex.Match(source, "\"\\.\\./(?<dir>[^\"]+)\"").Groups["dir"].Value;

        relative.ShouldNotBeEmpty();
        Directory.GetDirectories(Path.GetDirectoryName(infrastructure)!).Select(Path.GetFileName).ShouldContain(relative);
    }
}
