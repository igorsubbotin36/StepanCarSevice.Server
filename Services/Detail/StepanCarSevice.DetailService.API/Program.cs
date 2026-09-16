using StepanCarService.Common.API.AppExtensions;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarSevice.DetailService.Infrastructure;

namespace StepanCarSevice.DetailService.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = "Detail";
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            builder.AddSharedBulderSettings();
            builder.Services.AddInfrastructure(configuration);

            var app = builder.Build();

            app.UseSharedPipeline(builder.Configuration.GetConnectionString("PostgreSQL"));

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                await serviceProvider.MigrateDatabaseAsync();
            }

            await app.RunAsync();
        }
    }
}
