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
    }

    public static class RateLimitingSettings
    {
        public static IServiceCollection AddSharedRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("RateLimiting:Auth");
            var permitLimit = section.GetValue("PermitLimit", 10);
            var window = TimeSpan.FromSeconds(section.GetValue("WindowSeconds", 60));

            services.AddRateLimiter(options =>
            {
                // Лимит на IP клиента. За обратным прокси нужно настроить ForwardedHeaders,
                // иначе все запросы будут считаться с одного адреса прокси
                options.AddPolicy(RateLimitPolicies.Auth, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = window,
                            QueueLimit = 0
                        }));

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    var errorMapper = context.HttpContext.RequestServices.GetRequiredService<IErrorMapper>();
                    var (_, message) = errorMapper.Map(AuthErrors.TooManyRequests);
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { errorCode = AuthErrors.TooManyRequests, errorText = message },
                        cancellationToken);
                };
            });
            return services;
        }
    }
}
