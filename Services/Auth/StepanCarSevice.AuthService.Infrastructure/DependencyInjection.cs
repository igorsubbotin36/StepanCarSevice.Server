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
            services.AddSharedServices<AuthDbContext>(configuration);
            services.AddRabbitMQConsumer<AuthDbContext>(configuration);
            services.AddSharedMultitenantHostStrategy<AuthDbContext>();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenGeneratorService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthorizationService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
            services.AddSecurityStampValidation();

            return services;
        }

        // Токен, выданный до смены пароля, отклоняется: security_stamp в нём не совпадает с текущим.
        // Проверка работает в Auth, где хранятся пользователи; остальные сервисы принимают токен до истечения срока
        private static void AddSecurityStampValidation(this IServiceCollection services)
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var sharedValidation = options.Events.OnTokenValidated;
                options.Events.OnTokenValidated = async context =>
                {
                    await sharedValidation(context);
                    if (context.Result != null || context.Principal == null)
                        return;

                    var tokenStamp = context.Principal.FindFirst(AuthClaimTypes.SecurityStamp)?.Value;
                    if (!int.TryParse(context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
                        || string.IsNullOrEmpty(tokenStamp))
                    {
                        context.Fail("Token has no user id or security stamp.");
                        return;
                    }

                    var repository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                    var currentStamp = await repository.GetSecurityStampAsync(userId);
                    if (currentStamp == null || !string.Equals(currentStamp, tokenStamp, StringComparison.Ordinal))
                    {
                        context.Fail("Token has been revoked.");
                    }
                };
            });
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
