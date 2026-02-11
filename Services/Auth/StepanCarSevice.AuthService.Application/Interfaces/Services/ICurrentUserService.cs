using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
