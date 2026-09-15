
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StepanCarSevice.VisitService.Infrastructure.DBContexts.Factories
{
    public class VisitDbContextFactory : IDesignTimeDbContextFactory<VisitDBContext>
    {
        // Должен совпадать с <UserSecretsId> в StepanCarSevice.VisitService.API.csproj
        private const string UserSecretsId = "e441bb03-13be-4886-b009-1b7ffefb65c7";

        public VisitDBContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../StepanCarSevice.VisitService.API");

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

            var optionsBuilder = new DbContextOptionsBuilder<VisitDBContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new VisitDBContext(optionsBuilder.Options);
        }
    }
}
