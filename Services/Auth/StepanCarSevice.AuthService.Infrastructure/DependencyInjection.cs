using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Services;
using StepanCarSevice.AuthService.Domain.Repository;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using StepanCarSevice.AuthService.Infrastructure.DBContexts.Inits;
using StepanCarSevice.AuthService.Infrastructure.Repository;
using StepanCarSevice.AuthService.Infrastructure.Services;
using StepanCarSevice.AuthService.Infrastructure.Validation;
using System.Text;

namespace StepanCarSevice.AuthService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenGeneratorService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IErrorMapper, ErrorMapper>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            var connectionString = configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
            }

            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

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
                       ValidateLifetime = false, // Отключено для теста
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
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await dbContext.Database.MigrateAsync();
            DbInitializer.Init(dbContext);
        }
    }
}
