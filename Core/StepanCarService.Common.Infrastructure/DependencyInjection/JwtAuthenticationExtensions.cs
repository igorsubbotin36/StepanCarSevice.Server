using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        private const int MinJwtKeyBytes = 32;

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
            // HS256 требует ключ не короче 256 бит
            if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < MinJwtKeyBytes)
            {
                throw new InvalidOperationException(
                    $"JWT Key is too short: at least {MinJwtKeyBytes} bytes are required. " +
                    "Store the key in user-secrets or environment variables, not in appsettings.");
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
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidIssuer = jwtOptions.Issuer,
                       ValidateAudience = true,
                       ValidAudience = jwtOptions.Audience,
                       ValidateLifetime = true,
                       RequireExpirationTime = true,
                       // По умолчанию токен принимается ещё 5 минут после истечения
                       ClockSkew = TimeSpan.FromSeconds(30),
                       IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(jwtOptions.Key)),
                       ValidateIssuerSigningKey = true,
                       // Принимаем только алгоритм, которым Auth подписывает токены
                       ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
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

                           // 1. Текущий тенант из Finbuckle (null — запрос к порталу)
                           var accessor = context.HttpContext.RequestServices
                               .GetRequiredService<IMultiTenantContextAccessor<TenantInfoEntity>>();
                           var requestTenant = accessor.MultiTenantContext?.TenantInfo;
                           var requestTenantId = requestTenant?.Id;

                           // 2. tenant_id из токена (отсутствует у пользователей портала: GodMode и TenantOwner)
                           var tokenTenantId = principal.FindFirst("tenant_id")?.Value;
                           var tokenRole = principal.FindFirst(ClaimTypes.Role)?.Value;

                           // Неактивный тенант (например, закончилась подписка) доступен только GodMode
                           if (requestTenant != null && !requestTenant.IsActive && tokenRole != "GodMode")
                           {
                               context.Fail("Tenant is inactive.");
                               return;
                           }

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
                               if (tokenRole == "GodMode")
                               {
                                   if (!string.IsNullOrWhiteSpace(requestTenantId))
                                   {
                                       var identity = (ClaimsIdentity)principal.Identity!;
                                       identity.AddClaim(new Claim("tenant_id", requestTenantId));
                                   }
                               }
                               else if (tokenRole == "TenantOwner")
                               {
                                   if (requestTenant != null)
                                   {
                                       // Владелец входит на поддомен логином портала, но только в свой тенант
                                       var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                       if (requestTenant.OwnerUserId == null
                                           || userId != requestTenant.OwnerUserId.Value.ToString())
                                       {
                                           context.Fail("TenantOwner is not the owner of this tenant.");
                                           return;
                                       }
                                       var identity = (ClaimsIdentity)principal.Identity!;
                                       identity.AddClaim(new Claim("tenant_id", requestTenant.Id!));
                                   }
                               }
                               else
                               {
                                   context.Fail("Token missing tenant_id");
                                   return;
                               }
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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("GodModeOnly", policy =>
                    policy.RequireRole("GodMode"));


                options.AddPolicy("TenantOwnerInTenant", policy =>
                    policy.AddRequirements(new TenantRoleRequirement("GodMode", "TenantOwner")));

                options.AddPolicy("TenantModeratorInTenant", policy =>
                    policy.AddRequirements(new TenantRoleRequirement("GodMode", "TenantOwner", "TenantModerator")));

                options.AddPolicy("UserInTenant", policy =>
                    policy.AddRequirements(new TenantRoleRequirement("GodMode", "TenantOwner", "TenantModerator", "User")));
            });

            services.AddSingleton<IAuthorizationHandler, TenantRoleHandler>();
            return services;
        }
    }
}
