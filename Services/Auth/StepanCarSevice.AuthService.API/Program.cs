using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.AuthService.Infrastructure;

Console.Title = "Auth";
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

app.UseRouting();

app.UseMultiTenant();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await serviceProvider.MigrateDatabaseAsync();
}
app.Run();