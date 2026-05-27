using Microsoft.OpenApi.Models;
using NLog.Web;
using StepanCarService.Common.API.AppExtensions;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.TenantService.Infrastructure;

Console.Title = "Tenant";
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

app.Run();