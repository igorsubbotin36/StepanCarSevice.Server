using StepanCarSevice.AuthService.Domain.Entities;
using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Auth
{
    public static class AuthClaimTypes
    {
        public const string TenantId = "tenant_id";
        public const string SecurityStamp = "security_stamp";
    }

    public static class UserClaimsFactory
    {
        public static ClaimsIdentity BuildIdentity(User person)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, person.Phone ?? string.Empty),
                new Claim(ClaimTypes.Role, person.Role.Name),
                new Claim(ClaimTypes.Email, person.Email),
                new Claim(ClaimTypes.GivenName, person.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, person.SecondName ?? string.Empty),
                new Claim(AuthClaimTypes.SecurityStamp, person.SecurityStamp)
            };
            // У пользователей портала tenant_id нет: права владельца в тенанте проверяются при каждом запросе
            if (!string.IsNullOrEmpty(person.TenantId))
            {
                claims.Add(new Claim(AuthClaimTypes.TenantId, person.TenantId));
            }
            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
