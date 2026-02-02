using StepanCarSevice.AuthService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddInfrastructure(configuration);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    Environment.SetEnvironmentVariable("DB_CONNECTION_STRING",
    builder.Configuration.GetConnectionString("PostgreSQL"));
}

if (app.Environment.IsDevelopment())
{
    app.MapSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "My API V1");
    });
    //app.MapOpenApi();
    app.UseSwagger();
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await serviceProvider.MigrateDatabaseAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();