using System.Text;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StepanCarService.Core.Entities;
using StepanCarService.Core.Interfaces;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Services;
using StepanCarService.TenantService.Infrastructure.Auth;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.Web.Mappers;
using StepanCarService.Web.Repositories;

namespace StepanCarService.TenantService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IErrorMapper, ErrorMapper>();
        services.AddScoped<ITenantRepository, TenantBaseRepository<TenantServiceDbContext>>();
        services.AddScoped<ITenantService, TenantManagementService>();
        
        var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
            }
            services.AddDbContext<TenantServiceDbContext>(options => options.UseNpgsql(connectionString));
            services.AddMultiTenant<TenantInfoEntity>()
                .WithEFCoreStore<TenantServiceDbContext, TenantInfoEntity>()
                .WithStaticStrategy("management");

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
                           Console.WriteLine($"OnAuthenticationFailed: {context.Exception.Message}");
                           Console.WriteLine($"Exception details: {context.Exception}");
                           return Task.CompletedTask;
                       },
                       OnTokenValidated = context =>
                       {
                           Console.WriteLine("OnTokenValidated: Token is valid!");
                           return Task.CompletedTask;
                       },
                       OnChallenge = context =>
                       {
                           Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                           return Task.CompletedTask;
                       }
                   };
               });
        return services;
    }
    
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantServiceDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}