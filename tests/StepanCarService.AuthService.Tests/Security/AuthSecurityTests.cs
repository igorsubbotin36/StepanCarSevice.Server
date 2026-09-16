using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using StepanCarService.TestKit.MultiTenancy;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Application.Services;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using StepanCarSevice.AuthService.Infrastructure.Auth;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;
using static StepanCarService.AuthService.Tests.AuthTestUsers;

namespace StepanCarService.AuthService.Tests.Security;

// Auth с записью логов в память (FakeLogger) — для проверки, что в логи не попадают персональные данные и секреты
public class AuthServiceWithLogsFactory : AuthServiceFactory
{
    protected override void ConfigureTestServices(IServiceCollection services) =>
        services.AddFakeLogging();

    public FakeLogCollector Logs => Services.GetFakeLogCollector();
}

[Trait(TestCategories.Name, TestCategories.Security)]
[Collection(TestCollections.Database)]
public class AuthSecurityTests(AuthServiceWithLogsFactory factory)
    : ApiTestBase<AuthServiceWithLogsFactory, Program, AuthDbContext>(factory)
{
    // Подделки токена настоящего пользователя
    [Theory]
    [InlineData("alg: none")]
    [InlineData("HS512 тем же ключом")]
    [InlineData("подпись чужим ключом")]
    [InlineData("роль повышена без переподписи")]
    public async Task ForgedToken_Returns401(string forgery)
    {
        var owner = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.TenantOwner));
        var token = forgery switch
        {
            "alg: none" => TestTokenFactory.CreateUnsigned(Roles.GodMode, owner.Id),
            "HS512 тем же ключом" => TestTokenFactory.Create(Roles.TenantOwner, owner.Id, securityStamp: owner.SecurityStamp, algorithm: SecurityAlgorithms.HmacSha512),
            "подпись чужим ключом" => TestTokenFactory.Create(Roles.GodMode, owner.Id, securityStamp: owner.SecurityStamp,
                signingKey: Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")))),
            _ => ElevateRole(TokenFor(owner, Roles.TenantOwner))
        };
        using var client = Factory.CreateClientFor().WithBearer(token);

        (await client.GetAsync("api/User/getAllUsers")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized, forgery);
    }

    [Theory]
    [InlineData("evil.com")]
    [InlineData("tenant1.localhost.evil.com")]
    public async Task RequestWithForeignHost_Returns400(string host)
    {
        var context = await Factory.Server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/api/Auth/login";
            c.Request.Host = new HostString(host);
        });

        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    // Пустой Host пропускается host filtering (AllowEmptyHosts по умолчанию; HTTP/1.1 без Host отклоняет сам Kestrel),
    // но тенант не определяется — запрос обрабатывается как портал и в тенант не пускает
    [Fact]
    public async Task RequestWithEmptyHost_IsTreatedAsPortal()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var user = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id));
        var body = Encoding.UTF8.GetBytes($$"""{"phone":"{{user.Phone}}","password":"{{Password}}"}""");

        var context = await Factory.Server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/api/Auth/login";
            c.Request.Host = new HostString(string.Empty);
            c.Request.ContentType = "application/json";
            c.Request.Body = new MemoryStream(body);
        });

        context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    // Роль и тенант при регистрации определяет сервер (портал → владелец, поддомен → пользователь этого тенанта)
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Register_ExtraFieldsInBody_AreIgnored(bool onTenant)
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var foreignTenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        using var client = Factory.CreateClientFor(onTenant ? tenant.Identifier : null);
        var phone = TestData.Phone();

        var response = await client.PostAsJsonAsync("api/Auth/register", new
        {
            email = TestData.Email(), firstName = "Имя", secondName = "Фамилия", phone, password = Password, confirmPassword = Password,
            role = Roles.GodMode, roleId = RoleIds.GodMode, tenantId = foreignTenant.Id, id = 1, securityStamp = "forged"
        });

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        var user = await Factory.WithDbContextAsync(db => db.Users.SingleAsync(u => u.Phone == phone));
        user.RoleId.ShouldBe(onTenant ? RoleIds.User : RoleIds.TenantOwner);
        user.TenantId.ShouldBe(onTenant ? tenant.Id : null);
        user.SecurityStamp.ShouldNotBe("forged");
    }

    // Сценарии Auth не оставляют в логах телефонов, паролей, токенов, хэшей и меток
    [Fact]
    public async Task Logs_DoNotContainPersonalDataOrSecrets()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var godMode = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.GodMode));
        using var client = Factory.CreateClientFor(tenant.Identifier);
        var phone = TestData.Phone();
        var newPhone = TestData.Phone();
        const string newPassword = "New-password-1";
        Factory.Logs.Clear();

        await client.PostAsJsonAsync("api/Auth/register", new RegisterRequestDto(TestData.Email(), "Имя", "Фамилия", phone, Password, Password));
        await client.LoginAsync(phone, "Wrong-password");
        await client.LoginAsync(TestData.Phone());
        var token = await (await client.LoginAsync(phone)).ReadAccessTokenAsync();
        await client.WithBearer(token).PutAsJsonAsync("api/User/changePassword", new ChangePasswordRequestDto("Wrong-password", newPassword, newPassword));
        var newToken = await (await client.WithBearer(token).PutAsJsonAsync("api/User/changePassword",
            new ChangePasswordRequestDto(Password, newPassword, newPassword))).ReadAccessTokenAsync();
        await client.WithBearer(token).GetAsync("api/Auth/getTokenClaims");
        await client.WithBearer(newToken).PatchAsJsonAsync("api/User/updateUser", new EditUserRequestDto(null, null, null, newPhone));
        using var portal = Factory.CreateClientFor().WithBearer(TokenFor(godMode, Roles.GodMode));
        await portal.GetAsync($"api/User/getUserInfo?phone={Uri.EscapeDataString(newPhone)}");

        var user = await Factory.WithDbContextAsync(db => db.Users.SingleAsync(u => u.Phone == newPhone));
        string[] secrets =
        [
            phone, phone.TrimStart('+'), newPhone, newPhone.TrimStart('+'), Uri.EscapeDataString(newPhone),
            Password, newPassword, "Wrong-password", token, newToken, user.Password, user.SecurityStamp, godMode.SecurityStamp
        ];
        var records = Factory.Logs.GetSnapshot();

        records.ShouldNotBeEmpty();
        foreach (var record in records)
        {
            var text = $"{record.Message} {record.Exception}";
            secrets.Where(text.Contains).ShouldBeEmpty($"[{record.Level}] {record.Category}: {text}");
        }
    }

    // Ответы API не содержат хэшей паролей и меток безопасности
    [Fact]
    public async Task Responses_DoNotExposePasswordHashOrSecurityStamp()
    {
        var tenant = await Factory.SeedTenantAsync(TenantBuilder.Tenant());
        var godMode = await Factory.SeedUserAsync(UserBuilder.User().WithRole(RoleIds.GodMode));
        var moderator = await Factory.SeedUserAsync(UserBuilder.User().InTenant(tenant.Id).WithRole(RoleIds.TenantModerator));
        using var portal = Factory.CreateClientFor().WithBearer(TokenFor(godMode, Roles.GodMode));
        using var tenantClient = Factory.CreateClientFor(tenant.Identifier).WithBearer(TokenFor(moderator, Roles.TenantModerator));

        var bodies = new List<string>
        {
            await (await portal.GetAsync($"api/User/getUserInfo?phone={Uri.EscapeDataString(godMode.Phone!)}")).Content.ReadAsStringAsync(),
            await (await portal.GetAsync("api/User/getAllUsers")).Content.ReadAsStringAsync(),
            await (await tenantClient.GetAsync("api/User/getAllUsers")).Content.ReadAsStringAsync(),
            await (await tenantClient.WithBearer(null).LoginAsync(moderator.Phone!)).Content.ReadAsStringAsync()
        };
        var claims = await (await tenantClient.WithBearer(TokenFor(moderator, Roles.TenantModerator)).GetAsync("api/Auth/getTokenClaims")).Content.ReadAsStringAsync();

        bodies.ShouldAllBe(body => !body.Contains(godMode.Password) && !body.Contains(moderator.SecurityStamp) && !body.Contains(godMode.SecurityStamp)
            && !body.Contains("password", StringComparison.OrdinalIgnoreCase) && !body.Contains("securityStamp", StringComparison.OrdinalIgnoreCase));
        // Список claims показывает содержимое токена самого вызывающего (в том числе его security_stamp), но не хэш пароля
        claims.ShouldNotContain(moderator.Password);
    }

    // Время ответа «телефона нет» и «пароль неверный» почти одинаковое (хэш проверяется в обоих случаях)
    [Fact]
    public async Task Login_UnknownPhoneAndWrongPassword_TakeSimilarTime()
    {
        var hasher = new PasswordHasher();
        var owner = new User { Id = 1, Name = "owner", Password = hasher.Hash(Password), Role = new Role { Name = Roles.TenantOwner, Description = "" } };
        var users = Substitute.For<IUserRepository>();
        users.GetUserByPhoneAsync("+70000000001", null).Returns(owner);
        var service = new AuthorizationService(users, hasher, Substitute.For<ITokenGeneratorService>(), Substitute.For<ICurrentUserService>(),
            NullLogger<AuthorizationService>.Instance, new FakeTenantAccessor(), Substitute.For<IUnitOfWork>());

        async Task<TimeSpan> MeasureAsync(string phone)
        {
            await service.FindUserForLoginAsync(phone, "Wrong-password");
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 5; i++)
                (await service.FindUserForLoginAsync(phone, "Wrong-password")).ShouldBeNull();
            return stopwatch.Elapsed;
        }

        var wrongPassword = await MeasureAsync("+70000000001");
        var unknownPhone = await MeasureAsync("+70000000002");

        var ratio = Math.Max(wrongPassword.TotalMilliseconds, unknownPhone.TotalMilliseconds)
            / Math.Min(wrongPassword.TotalMilliseconds, unknownPhone.TotalMilliseconds);
        ratio.ShouldBeLessThan(2.0, $"неверный пароль: {wrongPassword.TotalMilliseconds:0} мс, нет телефона: {unknownPhone.TotalMilliseconds:0} мс");
    }

    private static string ElevateRole(string token)
    {
        var parts = token.Split('.');
        var payload = Base64UrlEncoder.Decode(parts[1]).Replace($"\"{Roles.TenantOwner}\"", $"\"{Roles.GodMode}\"");
        return $"{parts[0]}.{Base64UrlEncoder.Encode(payload)}.{parts[2]}";
    }
}
