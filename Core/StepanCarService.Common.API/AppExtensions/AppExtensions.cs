using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DependencyInjection;
using System.Net;

namespace StepanCarService.Common.API.AppExtensions
{
    public static class AppExtensions
    {
        public static WebApplication UseSharedPipeline(this WebApplication app, string connectionString)
        {
            if (app.Environment.IsDevelopment())
            {
                Environment.SetEnvironmentVariable("DB_CONNECTION_STRING",
                connectionString);
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.RoutePrefix = string.Empty;
                });
            }
            else
            {
                EnsureAllowedHostsConfigured(app);
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseRateLimiter();

            app.UseMultiTenant();

            if (app.Services.GetService<HostStrategyMarker>() != null)
            {
                app.UseUnknownTenantSubdomainGuard();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            return app;
        }

        // Тенант определяется по заголовку Host, поэтому вне Development
        // список разрешённых хостов обязан быть задан явно
        private static void EnsureAllowedHostsConfigured(WebApplication app)
        {
            var allowedHosts = app.Configuration["AllowedHosts"];
            var hosts = (allowedHosts ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (hosts.Length == 0 || hosts.Contains("*"))
            {
                throw new InvalidOperationException(
                    "AllowedHosts must list explicit host names outside Development " +
                    "(e.g. \"example.com;*.example.com\"). Wildcard \"*\" is not allowed.");
            }
        }

        private static void UseUnknownTenantSubdomainGuard(this WebApplication app)
        {
            var portalHosts = GetPortalHosts(app.Configuration["AllowedHosts"]);
            app.Use(async (context, next) =>
            {
                if (!IsPortalRequest(context.Request.Host.Host, portalHosts))
                {
                    var tenantContext = context.GetMultiTenantContext<TenantInfoEntity>();
                    if (tenantContext?.TenantInfo == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                }
                await next(context);
            });
        }

        private static HashSet<string> GetPortalHosts(string? allowedHosts) =>
            (allowedHosts ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(host => !host.StartsWith("*.", StringComparison.Ordinal))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private static bool IsPortalRequest(string host, HashSet<string> portalHosts) =>
            string.IsNullOrEmpty(host) || portalHosts.Contains(host) || IPAddress.TryParse(host, out _);
    }
}
