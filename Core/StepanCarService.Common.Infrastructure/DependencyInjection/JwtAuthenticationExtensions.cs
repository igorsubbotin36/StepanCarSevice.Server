using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.Auth;
using System.Security.Claims;
using System.Text;

namespace StepanCarService.Common.Infastructure.DependencyInjection
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddSharedJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var logger = LogManager.GetCurrentClassLogger();
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
                       OnTokenValidated = async context =>
                       {
                           var principal = context.Principal;
                           if (principal == null)
                           {
                               context.Fail("No principal.");
                               return;
                           }

                           // 1. Текущий тенант из Finbuckle
                           var accessor = context.HttpContext.RequestServices
                               .GetRequiredService<IMultiTenantContextAccessor<TenantInfoEntity>>();
                           var requestTenantId = accessor.MultiTenantContext?.TenantInfo?.Id;

                           // 2. tenant_id из токена (может отсутствовать у GodMode)
                           var tokenTenantId = principal.FindFirst("tenant_id")?.Value;

                           if (!string.IsNullOrWhiteSpace(tokenTenantId))
                           {
                               // Обычный пользователь: строгое соответствие
                               if (string.IsNullOrWhiteSpace(requestTenantId) ||
                                   !tokenTenantId.Equals(requestTenantId, StringComparison.OrdinalIgnoreCase))
                               {
                                   context.Fail("Tenant mismatch: token tenant does not match request tenant.");
                                   return;
                               }
                           }
                           else
                           {
                               // Токен без tenant_id – должен быть GodMode
                               var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                               if (role != "GodMode")
                               {
                                   context.Fail("Token is missing tenant_id and user is not GodMode.");
                                   return;
                               }

                               // Подставляем текущий tenant_id в claims для downstream-логики
                               if (!string.IsNullOrWhiteSpace(requestTenantId))
                               {
                                   var identity = (ClaimsIdentity)principal.Identity!;
                                   identity.AddClaim(new Claim("tenant_id", requestTenantId));
                               }
                               // если requestTenantId == null (например, запрос без поддомена), GodMode всё равно может работать без tenant_id?
                               // Решайте по бизнес-требованиям: можно пропустить или Fail.
                           }

                           logger.Info($"Tenant check OK. User: {principal.Identity?.Name}, Tenant: {requestTenantId ?? "none"}");
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
    }
}
