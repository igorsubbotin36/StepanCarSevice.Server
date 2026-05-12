using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Detail
    {
        [Key]
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string OriginalCode { get; set; }
        public required int DetailManufactureId { get; set; }
        public required DetailManufacture DetailManufacture { get; set; }
        public required string Name { get; set; }
        public required int CarModelId { get; set; }
        public required CarModel CarModel { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
