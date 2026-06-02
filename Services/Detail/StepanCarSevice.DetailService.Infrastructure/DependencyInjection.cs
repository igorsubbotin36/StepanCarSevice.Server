using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.Common.Infastructure.DependencyInjection;
using StepanCarService.Common.Infastructure.Messaging;
using StepanCarService.Common.Infastructure.Messaging.Handlers;
using StepanCarService.Common.Infastructure.Repositories;
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

            services.AddSharedServices<DetailDbContext>(configuration);
            services.AddRabbitMQConsumer<DetailDbContext>(configuration);
            services.AddSharedMultitenantHostStrategy<DetailDbContext>();

            //services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICarManufactureRepository, CarManufactureRepository>();
            services.AddScoped<ICarModelRepository, CarModelRepository>();
            services.AddScoped<IDetailManufactureRepository, DetailManufactureRepository>();
            services.AddScoped<IDetailRepository, DetailRepository>();
            services.AddScoped<IDetailService, DetailInteractionService>();

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
