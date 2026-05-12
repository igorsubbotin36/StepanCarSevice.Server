using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StepanCarService.Core.Entities;
using StepanCarService.Core.Interfaces;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.Web.Mappers;
using StepanCarService.Web.Repositories;
using StepanCarSevice.VisitService.Application.Interfaces;
using StepanCarSevice.VisitService.Application.Services;
using StepanCarSevice.VisitService.Domain.Repositories;
using StepanCarSevice.VisitService.Infrastructure.Auth;
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
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ICarRepository, CarRepository>();
            services.AddScoped(typeof(ITService<>), typeof(TService<>));
            services.AddScoped<ICarService, CarService>();
            services.AddScoped<ITenantRepository, TenantBaseRepository<VisitDBContext>>();

            services.AddScoped<IErrorMapper, ErrorMapper>();


            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
            }

            services.AddDbContext<VisitDBContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddMultiTenant<TenantInfoEntity>()
                .WithHostStrategy()
                .WithEFCoreStore<VisitDBContext, TenantInfoEntity>();

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
            var dbContext = scope.ServiceProvider.GetRequiredService<VisitDBContext>();
            await dbContext.Database.MigrateAsync();
            //DbInitializer.Init(dbContext);
        }
    }
}
