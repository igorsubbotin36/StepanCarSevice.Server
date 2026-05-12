using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class Work
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int VisitId { get; set; }
        public Visit Visit { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
