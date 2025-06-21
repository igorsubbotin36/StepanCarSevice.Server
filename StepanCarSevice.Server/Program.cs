using Microsoft.EntityFrameworkCore;
using StepanCarSevice.Server.DbContexts;
using NLog;
using NLog.Web;
var logger = LogManager.Setup().GetCurrentClassLogger();
logger.Debug("Start program");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen();
    string connectionString = "host=localhost;port=5432;database=CerService;User Id=postgres;password=q1w2e3r4";
    builder.Services.AddDbContext<PostgreDbContext>(options => options.UseNpgsql(connectionString));
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
}