using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StepanCarSevice.DetailService.Infrastructure.DBContexts.Factories
{
    public class DetailDbContextFactory : IDesignTimeDbContextFactory<DetailDbContext>
    {
        public DetailDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../StepanCarSevice.DetailService.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSQL");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Connection string 'PostgreSQL' not found for environment '{environment}'.");

            var optionsBuilder = new DbContextOptionsBuilder<DetailDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new DetailDbContext(optionsBuilder.Options);
        }
    }
}
