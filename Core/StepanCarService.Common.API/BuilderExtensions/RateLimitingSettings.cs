using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;

namespace StepanCarService.Common.API.BuilderExtensions
{
    public static class RateLimitPolicies
    {
        // Вход, регистрация, смена пароля — защита от перебора паролей
        public const string Auth = "auth";
        // Публичные эндпоинты без авторизации — защита от заваливания запросами
        public const string Public = "public";
    }

    public static class RateLimitingSettings
    {
        public static IServiceCollection AddSharedRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                // Лимиты на IP клиента. За обратным прокси нужно настроить ForwardedHeaders,
                // иначе все запросы будут считаться с одного адреса прокси
                AddPerIpPolicy(options, RateLimitPolicies.Auth, configuration.GetSection("RateLimiting:Auth"), defaultPermitLimit: 10);
                AddPerIpPolicy(options, RateLimitPolicies.Public, configuration.GetSection("RateLimiting:Public"), defaultPermitLimit: 60);

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    var errorMapper = context.HttpContext.RequestServices.GetRequiredService<IErrorMapper>();
                    var (_, message) = errorMapper.Map(SystemErrors.TooManyRequests);
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { errorCode = SystemErrors.TooManyRequests, errorText = message },
                        cancellationToken);
                };
            });
            return services;
        }

        private static void AddPerIpPolicy(RateLimiterOptions options, string policyName, IConfigurationSection section, int defaultPermitLimit)
        {
            var permitLimit = section.GetValue("PermitLimit", defaultPermitLimit);
            var window = TimeSpan.FromSeconds(section.GetValue("WindowSeconds", 60));
            options.AddPolicy(policyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0
                    }));
        }
    }
}
