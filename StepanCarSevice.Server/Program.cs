using Microsoft.EntityFrameworkCore;
using StepanCarSevice.Server.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
string connectionString = "host=localhost;port=5432;database=CerService;User Id=postgres;password=q1w2e3r4";
builder.Services.AddDbContext<PostgreDbContext>(options => options.UseNpgsql(connectionString));


var app = builder.Build();
await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();
db.Database.EnsureCreated();
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