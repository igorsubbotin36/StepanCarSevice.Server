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
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Services;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TenantService.Infrastructure.Messaging;
using StepanCarService.Web.DependencyInjection;
using StepanCarService.Web.Mappers;
using StepanCarService.Web.Repositories;
using System.Text;

namespace StepanCarService.TenantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var logger = LogManager.GetCurrentClassLogger();
        services.AddSharedServices();
        services.AddScoped<ITenantRepository, TenantBaseRepository<TenantServiceDbContext>>();
        services.AddScoped<ITenantService, TenantManagementService>();

        services.AddSharedPostgreSQL<TenantServiceDbContext>(configuration);

        services.AddMultiTenant<TenantInfoEntity>()
                .WithEFCoreStore<TenantServiceDbContext, TenantInfoEntity>()
                .WithStaticStrategy("management");

        services.Configure<RabbitMQProducerSettings>(
                configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IMessageBus, RabbitMQBus>();

        services.AddSharedJwtAuthentication(configuration);
        return services;
    }
    
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantServiceDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}