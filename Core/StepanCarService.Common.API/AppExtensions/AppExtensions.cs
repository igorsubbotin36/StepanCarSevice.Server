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
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.RoutePrefix = string.Empty;
                });
            }
            app.MapOpenApi();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMultiTenant();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            return app;
        }
    }
}
