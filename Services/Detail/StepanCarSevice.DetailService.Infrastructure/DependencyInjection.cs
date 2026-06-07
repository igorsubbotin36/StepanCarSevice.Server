using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using StepanCarService.Common.Infastructure.DependencyInjection;
using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Models.Mappers;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using StepanCarSevice.DetailService.Infrastructure.DBContexts.Inits;
using StepanCarSevice.DetailService.Infrastructure.Repositories;

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

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICarModelRepository, CarModelRepository>();
            services.AddScoped<IDetailRepository, DetailRepository>();
            //services.AddScoped<IDetailService, DetailInteractionService>();
            services.AddScoped<IDetailMappers, DetailMappers>();

            return services;
        }
        public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DetailDbContext>();
            await dbContext.Database.MigrateAsync();
            DbInitializer.Init(dbContext);
        }
    }
}
