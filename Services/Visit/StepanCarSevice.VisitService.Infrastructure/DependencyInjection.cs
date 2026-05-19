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
using StepanCarSevice.VisitService.Application.Interfaces;
using StepanCarSevice.VisitService.Application.Services;
using StepanCarSevice.VisitService.Domain.Repositories;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;
using StepanCarSevice.VisitService.Infrastructure.Repositories;
using System.Text;

namespace StepanCarSevice.VisitService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var logger = LogManager.GetCurrentClassLogger();

            services.AddSharedServices();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICarRepository, CarRepository>();
            services.AddScoped(typeof(ITService<>), typeof(TService<>));
            services.AddScoped<ICarService, CarService>();
            services.AddScoped<ITenantRepository, TenantBaseRepository<VisitDBContext>>();

            services.Configure<RabbitMQConsumerSetting>(
                configuration.GetSection("RabbitMQ"));
            services.AddScoped<TenantBaseRepository<VisitDBContext>>();
            services.AddScoped<ITenantRegisteredHandler, TenantRegisteredHandler<TenantBaseRepository<VisitDBContext>>>();
            services.AddHostedService<TenantEventsConsumer>();

            services.AddSharedPostgreSQL<VisitDBContext>(configuration);

            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithEFCoreStore<VisitDBContext, TenantInfoEntity>();

            services.AddSharedJwtAuthentication(configuration);

            return services;
        }
        public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<VisitDBContext>();
            await dbContext.Database.MigrateAsync();
            //DbInitializer.Init(dbContext);
        }
    }
}
