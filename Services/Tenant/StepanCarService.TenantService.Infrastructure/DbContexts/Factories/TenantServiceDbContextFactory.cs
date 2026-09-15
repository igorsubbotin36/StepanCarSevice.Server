using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StepanCarService.TenantService.Infrastructure.DbContexts.Factories;

public class TenantServiceDbContextFactory : IDesignTimeDbContextFactory<TenantServiceDbContext>
{
    // Должен совпадать с <UserSecretsId> в StepanCarService.TenantService.API.csproj
    private const string UserSecretsId = "2ffc3358-4bb2-49b3-82a9-23733c6998b5";

    public TenantServiceDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../StepanCarService.TenantService.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(UserSecretsId)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                $"Connection string 'PostgreSQL' not found for environment '{environment}'.");

        var optionsBuilder = new DbContextOptionsBuilder<TenantServiceDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TenantServiceDbContext(optionsBuilder.Options);
    }
}