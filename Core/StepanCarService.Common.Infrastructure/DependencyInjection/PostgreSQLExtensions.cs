using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Infastructure.DbContexts;

namespace StepanCarService.Common.Infastructure.DependencyInjection
{
    public static class PostgreSQLExtensions
    {
        public static IServiceCollection AddSharedPostgreSQL<T>(
        this IServiceCollection services,
        IConfiguration configuration) where T : TenantBaseDbContext
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
            }

            services.AddDbContext<T>(options => options.UseNpgsql(connectionString));
            return services;
        }
    }
}
