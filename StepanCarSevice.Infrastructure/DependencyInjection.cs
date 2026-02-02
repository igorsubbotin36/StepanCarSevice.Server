using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarSevice.Application.Auth;
using StepanCarSevice.Application.Repository.Interfaces;
using StepanCarSevice.Infrastructure.Auth;
using StepanCarSevice.Infrastructure.DbContexts;
using StepanCarSevice.Infrastructure.Repository.PostgreRepository;

namespace StepanCarSevice.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            services.AddDbContext<PostgreDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("PostgreSQL");
                options.UseNpgsql(connectionString);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDetailRepository, DetailRepository>();

            return services;
        }
    }
}


