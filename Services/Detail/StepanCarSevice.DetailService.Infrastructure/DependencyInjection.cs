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
using StepanCarService.Web.Mappers;
using StepanCarService.Web.Messaging;
using StepanCarService.Web.Messaging.Handlers;
using StepanCarService.Web.Repositories;
using StepanCarSevice.AuthService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Services;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.Auth;
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
            services.AddScoped<IErrorMapper, ErrorMapper>();
            services.AddScoped<ITenantRepository, TenantBaseRepository<DetailDbContext>>();
            services.AddHostedService<TenantEventsConsumer>();

            services.Configure<RabbitMQConsumerSetting>(
                configuration.GetSection("RabbitMQ"));
            services.AddScoped<TenantBaseRepository<DetailDbContext>>();
            services.AddScoped<ITenantRegisteredHandler, TenantRegisteredHandler<TenantBaseRepository<DetailDbContext>>>();
            services.AddHostedService<TenantEventsConsumer>();

            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
            }

            services.AddDbContext<DetailDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithEFCoreStore<DetailDbContext, TenantInfoEntity>();

            var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
            if (jwtOptions == null)
            {
                throw new InvalidOperationException("JWT configuration section is missing.");
            }

            // Проверяем, что все обязательные поля заполнены
            if (string.IsNullOrWhiteSpace(jwtOptions.Key))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured.");
            }
            if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
            {
                throw new InvalidOperationException("JWT Audience is not configured.");
            }
            if (jwtOptions.LifetimeMinutes <= 0)
            {
                jwtOptions.LifetimeMinutes = 60; // значение по умолчанию
            }

            services.AddSingleton<JwtOptions>(jwtOptions);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
               {
                   options.RequireHttpsMetadata = false;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidIssuer = jwtOptions.Issuer,
                       ValidateAudience = true,
                       ValidAudience = jwtOptions.Audience,
                       ValidateLifetime = true,
                       IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(jwtOptions.Key)),
                       ValidateIssuerSigningKey = true,
                   };

                   // ВАЖНО: Добавьте обработчики событий для отладки
                   options.Events = new JwtBearerEvents
                   {
                       OnAuthenticationFailed = context =>
                       {
                           logger.Error($"OnAuthenticationFailed: {context.Exception.Message}\nException details: {context.Exception}");
                           return Task.CompletedTask;
                       },
                       OnTokenValidated = context =>
                       {
                           logger.Info("OnTokenValidated: Token is valid!");
                           return Task.CompletedTask;
                       },
                       OnChallenge = context =>
                       {
                           logger.Error($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                           return Task.CompletedTask;
                       }
                   };
               });
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
