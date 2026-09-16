using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;
using static StepanCarService.Common.Tests.Api.SharedPipelineHost;

namespace StepanCarService.Common.Tests.Api;

public sealed class SharedPipelineFixture : IAsyncLifetime
{
    public SharedPipelineHost Host { get; private set; } = null!;

    public async ValueTask InitializeAsync() => Host = await StartAsync();

    public ValueTask DisposeAsync() => Host.DisposeAsync();
}

// Проверка токена и тенанта запроса (AddSharedJwtAuthentication → OnTokenValidated) и политики авторизации.
// Данные [Theory] — только описатели: xUnit вычисляет их при discovery, где ключ JWT и Id тенантов другие
[Trait(TestCategories.Name, TestCategories.Api)]
public class JwtPipelineTests(SharedPipelineFixture fixture) : IClassFixture<SharedPipelineFixture>
{
    private const string AuthenticatedPath = "probe/authenticated";

    private async Task<HttpResponseMessage> GetAsync(string path, string token, TenantInfoEntity? tenant = null)
    {
        using var client = fixture.Host.CreateClient(tenant?.Identifier).WithBearer(token);
        return await client.GetAsync(path);
    }

    [Fact]
    public async Task ValidHs256Token_Returns200()
    {
        var response = await GetAsync(AuthenticatedPath, TestTokenFactory.Create(Roles.GodMode));

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Поддельные и недействительные токены GodMode на портале
    [Theory]
    [InlineData("подпись другим ключом")]
    [InlineData("тот же ключ, HS512")]
    [InlineData("alg: none")]
    [InlineData("роль повышена в payload без переподписи")]
    [InlineData("чужой iss")]
    [InlineData("чужой aud")]
    [InlineData("истёк 90 секунд назад")]
    [InlineData("нет exp")]
    [InlineData("nbf через 5 минут")]
    public async Task InvalidToken_Returns401(string tokenCase)
    {
        var token = tokenCase switch
        {
            "подпись другим ключом" => TestTokenFactory.Create(Roles.GodMode,
                signingKey: Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")))),
            "тот же ключ, HS512" => TestTokenFactory.Create(Roles.GodMode, algorithm: SecurityAlgorithms.HmacSha512),
            "alg: none" => TestTokenFactory.CreateUnsigned(Roles.GodMode),
            "роль повышена в payload без переподписи" => ElevateRoleWithoutResigning(TestTokenFactory.Create(Roles.User)),
            "чужой iss" => TestTokenFactory.Create(Roles.GodMode, issuer: "someone-else"),
            "чужой aud" => TestTokenFactory.Create(Roles.GodMode, audience: "someone-else"),
            "истёк 90 секунд назад" => TestTokenFactory.Create(Roles.GodMode, expiresIn: TimeSpan.FromSeconds(-90)),
            "нет exp" => TestTokenFactory.CreateWithoutExpiration(Roles.GodMode),
            "nbf через 5 минут" => TestTokenFactory.Create(Roles.GodMode, notBefore: DateTime.UtcNow.AddMinutes(5)),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenCase))
        };

        var response = await GetAsync(AuthenticatedPath, token);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, tokenCase);
    }

    // Допуск расхождения часов — 30 секунд
    [Fact]
    public async Task TokenExpired10SecondsAgo_IsStillAccepted()
    {
        var response = await GetAsync(AuthenticatedPath, TestTokenFactory.Create(Roles.GodMode, expiresIn: TimeSpan.FromSeconds(-10)));

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Роль × tenant_id токена × тенант запроса (активность, владелец).
    // Тенанты: A (владелец 7), B (владелец 8), C (без владельца), Inactive (владелец 7, неактивен); "-" — нет / портал
    [Theory]
    [InlineData(Roles.User, "A", "A", HttpStatusCode.OK, "A")]
    [InlineData(Roles.User, "A", "B", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.User, "A", "-", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.User, "Inactive", "Inactive", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.TenantModerator, "A", "A", HttpStatusCode.OK, "A")]
    [InlineData(Roles.GodMode, "-", "-", HttpStatusCode.OK, "-")]
    [InlineData(Roles.GodMode, "-", "A", HttpStatusCode.OK, "A")]
    [InlineData(Roles.GodMode, "-", "Inactive", HttpStatusCode.OK, "Inactive")]
    [InlineData(Roles.TenantOwner, "-", "-", HttpStatusCode.OK, "-")]
    // Владелец в своём тенанте: tenant_id доступен в OnTokenValidated — UseMultiTenant стоит до UseAuthentication
    [InlineData(Roles.TenantOwner, "-", "A", HttpStatusCode.OK, "A")]
    [InlineData(Roles.TenantOwner, "-", "B", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.TenantOwner, "-", "C", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.TenantOwner, "-", "Inactive", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.TenantOwner + ":без nameidentifier", "-", "A", HttpStatusCode.Unauthorized, "-")]
    [InlineData(Roles.User, "-", "-", HttpStatusCode.Unauthorized, "-")]
    [InlineData("-", "-", "-", HttpStatusCode.Unauthorized, "-")]
    public async Task TenantAccess(string role, string tokenTenant, string requestTenant,
        HttpStatusCode expectedStatus, string expectedTenantId)
    {
        var token = role switch
        {
            "-" => TestTokenFactory.Create(role: null),
            Roles.TenantOwner + ":без nameidentifier" => TestTokenFactory.Create(Roles.TenantOwner, includeUserId: false),
            _ => Token(role, TenantByKey(tokenTenant))
        };

        var response = await GetAsync(AuthenticatedPath, token, TenantByKey(requestTenant));
        var caseName = $"роль {role}, tenant_id {tokenTenant}, запрос к {requestTenant}";

        response.StatusCode.ShouldBe(expectedStatus, caseName);
        if (expectedStatus == HttpStatusCode.OK)
            (await response.Content.ReadFromJsonAsync<ProbeIdentity>())!.TenantId.ShouldBe(TenantByKey(expectedTenantId)?.Id, caseName);
    }

    // Иерархические политики; «запрещено» — 401 (тенант не прошёл проверку токена) или 403
    public static TheoryData<string, string, string> PolicyMatrix()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var policy in Policies)
            foreach (var role in new[] { Roles.GodMode, Roles.TenantOwner, Roles.TenantModerator, Roles.User })
                foreach (var location in new[] { "own", "foreign", "portal" })
                    data.Add(policy, role, location);
        return data;
    }

    [Theory]
    [MemberData(nameof(PolicyMatrix))]
    public async Task Policy(string policy, string role, string location)
    {
        // Владелец (id 7) владеет TenantA; модератор и пользователь зарегистрированы в TenantA
        var token = role is Roles.GodMode or Roles.TenantOwner ? Token(role) : Token(role, TenantA);
        var tenant = location switch { "own" => TenantA, "foreign" => TenantB, _ => null };
        string[] rolesInTenant = policy switch
        {
            "TenantOwnerInTenant" => [Roles.TenantOwner],
            "TenantModeratorInTenant" => [Roles.TenantOwner, Roles.TenantModerator],
            "UserInTenant" => [Roles.TenantOwner, Roles.TenantModerator, Roles.User],
            _ => []
        };
        var allowed = role == Roles.GodMode || (location == "own" && rolesInTenant.Contains(role));

        var response = await GetAsync($"probe/policy/{policy}", token, tenant);

        if (allowed)
            await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        else
            response.StatusCode.ShouldBeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    private static TenantInfoEntity? TenantByKey(string key) => key switch
    {
        "A" => TenantA,
        "B" => TenantB,
        "C" => TenantWithoutOwner,
        "Inactive" => InactiveTenant,
        "-" => null,
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };

    private static string Token(string role, TenantInfoEntity? tenant = null) =>
        TestTokenFactory.Create(role, userId: int.Parse(OwnerUserId), tenantId: tenant?.Id);

    // Повышает роль в payload до GodMode, оставляя старую подпись
    private static string ElevateRoleWithoutResigning(string token)
    {
        var parts = token.Split('.');
        var payload = Base64UrlEncoder.Decode(parts[1]).Replace($"\"{Roles.User}\"", $"\"{Roles.GodMode}\"");
        payload.ShouldContain(Roles.GodMode);
        return $"{parts[0]}.{Base64UrlEncoder.Encode(payload)}.{parts[2]}";
    }
}
