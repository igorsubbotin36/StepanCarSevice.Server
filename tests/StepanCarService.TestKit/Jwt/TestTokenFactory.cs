using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace StepanCarService.TestKit.Jwt;

// Токены для тестов: те же claims и тот же способ записи, что в Auth (UserClaimsFactory + TokenService),
// а также заведомо поддельные варианты (чужой ключ, другой алгоритм, без подписи, истёкшие)
public static class TestTokenFactory
{
    public const string Issuer = "test-issuer";
    public const string Audience = "test-audience";
    public const string TenantIdClaim = "tenant_id";
    public const string SecurityStampClaim = "security_stamp";

    // Случайный ключ на прогон; тот же ключ получает сервис через ServiceFactory
    public static readonly string Key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));

    // role = null — токен без claim роли
    public static string Create(
        string? role,
        int userId = 1,
        string? tenantId = null,
        string? securityStamp = null,
        string phone = "+70000000000",
        TimeSpan? expiresIn = null,
        DateTime? notBefore = null,
        string algorithm = SecurityAlgorithms.HmacSha256,
        string? signingKey = null,
        string issuer = Issuer,
        string audience = Audience,
        bool includeUserId = true,
        IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.MobilePhone, phone) };
        if (role != null)
            claims.Add(new Claim(ClaimTypes.Role, role));
        if (includeUserId)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        if (securityStamp != null)
            claims.Add(new Claim(SecurityStampClaim, securityStamp));
        if (tenantId != null)
            claims.Add(new Claim(TenantIdClaim, tenantId));
        if (extraClaims != null)
            claims.AddRange(extraClaims);

        var now = DateTime.UtcNow;
        var expires = now + (expiresIn ?? TimeSpan.FromMinutes(30));
        // Для истёкшего токена nbf тоже в прошлом: библиотека требует nbf < exp
        var validFrom = notBefore ?? (expires <= now ? expires.AddMinutes(-10) : now);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: validFrom,
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? Key)), algorithm));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    // Токен без срока действия (exp отсутствует)
    public static string CreateWithoutExpiration(string role, int userId = 1)
    {
        var jwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    // Токен без подписи (alg: none) с теми же claims
    public static string CreateUnsigned(string role, int userId = 1, string? tenantId = null)
    {
        var payload = Create(role, userId, tenantId).Split('.')[1];
        var header = Base64UrlEncoder.Encode("""{"alg":"none","typ":"JWT"}""");
        return $"{header}.{payload}.";
    }
}
