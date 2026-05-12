using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class OwnerSnapshot
    {
        [Key]
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string SecondName { get; set; }
        public required string Phone { get; set; }
        public string? Email { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
