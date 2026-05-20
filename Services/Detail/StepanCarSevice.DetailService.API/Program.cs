using Microsoft.OpenApi.Models;
using NLog.Web;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarSevice.DetailService.Infrastructure;
namespace StepanCarSevice.DetailService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Detail";
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            builder.AddSharedBulderSettings();
            builder.Services.AddInfrastructure(configuration);

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                Environment.SetEnvironmentVariable("DB_CONNECTION_STRING",
                builder.Configuration.GetConnectionString("PostgreSQL"));
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

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                serviceProvider.MigrateDatabaseAsync().Wait();
            }

            app.Run();
        }
    }
}
