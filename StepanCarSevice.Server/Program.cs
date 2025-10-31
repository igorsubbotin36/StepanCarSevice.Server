using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using StepanCarService.Server.DbContexts;
using StepanCarService.Server.Auth;
using StepanCarService.Server.Repository.Interfaces;
using StepanCarService.Server.Repository.PostgreRepository;
using Microsoft.Extensions.Options;
using FluentValidation.AspNetCore;
using FluentValidation;
var logger = LogManager.Setup().GetCurrentClassLogger();
logger.Debug("Start program");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<StepanCarService.Server.Models.Dto.RegisterDto>();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen();
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
    string connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    builder.Services.AddDbContext<PostgreDbContext>(options => options.UseNpgsql(connectionString));
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                ValidateIssuerSigningKey = true,
            };
        });

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDetailRepository, DetailRepository>();

    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
}