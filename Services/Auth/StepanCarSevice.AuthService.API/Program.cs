using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using StepanCarService.Common.API.AppExtensions;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.AuthService.Infrastructure;

Console.Title = "Auth";
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

// Нужен тестам: WebApplicationFactory<Program> (top-level statements делают класс Program внутренним)
public partial class Program { }
