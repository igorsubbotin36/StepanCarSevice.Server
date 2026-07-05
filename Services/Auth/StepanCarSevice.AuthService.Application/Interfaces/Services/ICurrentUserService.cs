using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? MobilePhone { get; }
        string? Email { get; }
        string? FirstName { get; }
        string? LastName { get; }
        string? FullName { get; }
        string? Role { get; }
        ClaimsPrincipal? Principal { get; }
        bool IsAuthenticated { get; }
        T? GetClaimValue<T>(string claimType);
        bool HasClaim(string claimType, string value);
        IReadOnlyList<Claim> GetAllClaims();
    }
}
