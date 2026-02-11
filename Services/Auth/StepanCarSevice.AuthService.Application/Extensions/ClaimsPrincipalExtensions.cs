using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        private const string MobilePhoneClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";
        private const string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public static string GetMobilePhone(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(MobilePhoneClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.MobilePhone)?.Value
                ?? throw new InvalidOperationException("Mobile phone not found in claims");
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(EmailClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new InvalidOperationException("Email not found in claims");
        }

        public static string GetFirstName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("FirstName")?.Value
                ?? principal.FindFirst(ClaimTypes.GivenName)?.Value
                ?? string.Empty;
        }

        public static string GetSecondName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("SecondName")?.Value
                ?? principal.FindFirst(ClaimTypes.Surname)?.Value
                ?? string.Empty;
        }

        public static string GetFullName(this ClaimsPrincipal principal)
        {
            var firstName = principal.GetFirstName();
            var secondName = principal.GetSecondName();

            return string.IsNullOrEmpty(secondName)
                ? firstName
                : $"{firstName} {secondName}";
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value
                ?? principal.FindFirst("role")?.Value
                ?? throw new InvalidOperationException("Role not found in claims");
        }


        // Optional: Safe versions without exceptions
        public static string? TryGetMobilePhone(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(MobilePhoneClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.MobilePhone)?.Value;
        }

        public static string? TryGetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(EmailClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.Email)?.Value;
        }
    }

}
