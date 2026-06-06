using System.ComponentModel.DataAnnotations;

namespace StepanCarService.Common.Core.Entities
{
    abstract public class AppBaseEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
