using Microsoft.AspNetCore.Http;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public string? UserId => Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public string? MobilePhone => Principal?.FindFirst(ClaimTypes.MobilePhone)?.Value;
        public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value;
        public string? FirstName => Principal?.FindFirst(ClaimTypes.GivenName)?.Value;
        public string? LastName => Principal?.FindFirst(ClaimTypes.Surname)?.Value;
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public string? Role => Principal?.FindFirst(ClaimTypes.Role)?.Value;
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public IReadOnlyList<Claim> GetAllClaims()
        {
            return Principal?.Claims.ToList().AsReadOnly()
                ?? new List<Claim>().AsReadOnly();
        }

        public T? GetClaimValue<T>(string claimType)
        {
            var value = Principal?.FindFirst(claimType)?.Value;
            if (value == null) return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public bool HasClaim(string claimType, string value)
        {
            return Principal?.HasClaim(claimType, value) ?? false;
        }


    }
}
