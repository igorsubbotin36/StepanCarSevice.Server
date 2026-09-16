using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using StepanCarSevice.AuthService.Infrastructure.DBContexts.Inits;
using StepanCarService.TestKit.Databases;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.MultiTenancy;

namespace StepanCarService.AuthService.Tests;

// Auth целиком (API-тесты): миграции и сид ролей выполняет сам сервис при старте
public class AuthServiceFactory : ServiceFactory<Program, AuthDbContext>
{
    protected override string DatabasePrefix => "auth";
    protected override IEnumerable<string> TablesToKeep => ["Roles"];
}

// БД Auth без HTTP (интеграционные тесты репозиториев и схемы)
public class AuthDatabase : ServiceDatabase<AuthDbContext>
{
    protected override string Prefix => "auth";
    protected override IEnumerable<string> TablesToKeep => ["Roles"];

    // AuthDbContext не зависит от тенанта запроса
    public override AuthDbContext CreateContext(FakeTenantAccessor tenantAccessor) => new(CreateOptions());

    protected override Task SeedAsync(AuthDbContext db)
    {
        DbInitializer.Init(db);
        return Task.CompletedTask;
    }
}

// Пользователи и токены для API-тестов Auth
public static class AuthTestUsers
{
    public const string Password = "Password-1";

    private static readonly PasswordHasher Hasher = new();
    // PBKDF2 медленный: хэш общего пароля считается один раз
    private static readonly Lazy<string> PasswordHash = new(() => Hasher.Hash(Password));

    // Пользователь прямо в БД сервиса; password = null — общий пароль Password
    public static async Task<User> SeedUserAsync(this AuthServiceFactory factory, UserBuilder builder, string? password = null)
    {
        var user = builder.WithPasswordHash(password == null ? PasswordHash.Value : Hasher.Hash(password)).Build();
        await factory.WithDbContextAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        });
        return user;
    }

    public static Task<User?> FindUserAsync(this AuthServiceFactory factory, int id) =>
        factory.WithDbContextAsync(db => db.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id));

    // Токен, как его выдал бы Auth этому пользователю
    public static string TokenFor(User user, string role) =>
        TestTokenFactory.Create(role, user.Id, string.IsNullOrEmpty(user.TenantId) ? null : user.TenantId, user.SecurityStamp, user.Phone!);

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string phone, string password = Password) =>
        client.PostAsJsonAsync("api/Auth/login", new LoginRequestDto(phone, password));

    public static async Task<string> ReadAccessTokenAsync(this HttpResponseMessage response)
    {
        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>()).ShouldNotBeNull().AccessToken;
    }

    public static JwtSecurityToken ReadJwt(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    public static string? ClaimValue(this JwtSecurityToken jwt, string type) => jwt.Claims.FirstOrDefault(c => c.Type == type)?.Value;
}
