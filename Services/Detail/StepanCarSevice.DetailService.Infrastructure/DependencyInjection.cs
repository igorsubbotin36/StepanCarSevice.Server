using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using StepanCarService.Core.Entities;
using StepanCarService.Core.Interfaces;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.Web.DependencyInjection;
using StepanCarService.Web.Mappers;
using StepanCarService.Web.Messaging;
using StepanCarService.Web.Messaging.Handlers;
using StepanCarService.Web.Repositories;
using StepanCarSevice.AuthService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Services;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarSevice.DetailService.Infrastructure.Repositories;
using System.Text;

namespace StepanCarSevice.DetailService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var logger = LogManager.GetCurrentClassLogger();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IDetailRepository, DetailRepository>();
            services.AddScoped(typeof(ITService<>), typeof(TService<>));
            services.AddScoped<IDetailService, DetailInteractionService>();
            services.AddSharedServices();
            services.AddScoped<ITenantRepository, TenantBaseRepository<DetailDbContext>>();
            services.AddHostedService<TenantEventsConsumer>();

            services.Configure<RabbitMQConsumerSetting>(
                configuration.GetSection("RabbitMQ"));
            services.AddScoped<TenantBaseRepository<DetailDbContext>>();
            services.AddScoped<ITenantRegisteredHandler, TenantRegisteredHandler<TenantBaseRepository<DetailDbContext>>>();
            services.AddHostedService<TenantEventsConsumer>();

            services.AddSharedPostgreSQL<DetailDbContext>(configuration);

            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithEFCoreStore<DetailDbContext, TenantInfoEntity>();

            services.AddSharedJwtAuthentication(configuration);

            return services;
        }
        public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DetailDbContext>();
            await dbContext.Database.MigrateAsync();
            //DbInitializer.Init(dbContext);
        }
    }
}
