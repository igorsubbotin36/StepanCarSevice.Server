using StepanCarService.Common.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.AuthService.Domain.Entities
{
    public class User : AppBaseTenantsEntity
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
