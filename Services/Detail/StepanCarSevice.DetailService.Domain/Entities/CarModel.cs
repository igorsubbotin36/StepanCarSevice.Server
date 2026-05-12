using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;
namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarModel
    {
        [Key]
        public int Id { get; set; }
        public int ManufactureId { get; set; }
        public required CarManufacture Manufacture { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
