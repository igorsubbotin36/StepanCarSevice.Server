using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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

            app.UseMultiTenant();

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
    }
}
