using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

            services.AddDbContext<T>(options => options
                .UseNpgsql(connectionString)
                // Текст каждого выполненного SQL-запроса — только на уровне Debug (в обычном логе Info его нет).
                // Ошибки выполнения запросов (CommandError) по-прежнему логируются как Error
                .ConfigureWarnings(warnings => warnings.Log((RelationalEventId.CommandExecuted, LogLevel.Debug))));
            return services;
        }
    }
}
