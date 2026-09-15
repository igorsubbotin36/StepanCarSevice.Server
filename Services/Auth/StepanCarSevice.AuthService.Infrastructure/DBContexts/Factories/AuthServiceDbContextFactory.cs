using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Infrastructure.DBContexts.Factories
{
    public class AuthServiceDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        // Должен совпадать с <UserSecretsId> в StepanCarSevice.AuthService.API.csproj
        private const string UserSecretsId = "44eb5a33-857e-46b0-b233-2a5856610f32";

        public AuthDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../StepanCarSevice.AuthService.API");

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

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
