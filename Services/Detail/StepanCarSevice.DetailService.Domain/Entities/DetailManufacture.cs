using StepanCarService.Common.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class DetailManufacture
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
