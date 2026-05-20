using StepanCarService.Common.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarManufacture
    {
        [Key]
        public int Id { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public required string Country { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant {  get; set; }
    }
}
