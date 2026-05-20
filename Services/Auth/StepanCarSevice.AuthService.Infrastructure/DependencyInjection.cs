using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FluentValidation;
using FluentValidation.AspNetCore;
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
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Services;
using StepanCarSevice.AuthService.Domain.Repositories;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using StepanCarSevice.AuthService.Infrastructure.DBContexts.Inits;
using StepanCarSevice.AuthService.Infrastructure.Repositories;
using StepanCarSevice.AuthService.Infrastructure.Services;
using StepanCarSevice.AuthService.Infrastructure.Validation;
using System.Security.Claims;
using System.Text;

namespace StepanCarSevice.AuthService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var logger = LogManager.GetCurrentClassLogger();
            services.AddHttpContextAccessor();
            services.AddSharedServices();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenGeneratorService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthorizationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITenantRepository, TenantBaseRepository<AuthDbContext>>();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
            services.Configure<RabbitMQConsumerSetting>(
                configuration.GetSection("RabbitMQ"));
            services.AddScoped<TenantBaseRepository<AuthDbContext>>();
            services.AddScoped<ITenantRegisteredHandler, TenantRegisteredHandler<TenantBaseRepository<AuthDbContext>>>();
            services.AddHostedService<TenantEventsConsumer>();


            services.AddSharedPostgreSQL<AuthDbContext>(configuration);

            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithEFCoreStore<AuthDbContext, TenantInfoEntity>();

            services.AddSharedJwtAuthentication(configuration);

            return services;
        }
        public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await dbContext.Database.MigrateAsync();
            DbInitializer.Init(dbContext);
        }
    }
}
