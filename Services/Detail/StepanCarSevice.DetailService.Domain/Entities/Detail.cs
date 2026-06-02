using StepanCarService.Common.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Detail
    {
        [Key]
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string OriginalCode { get; set; }
        public int DetailManufactureId { get; set; }
        public DetailManufacture? DetailManufacture { get; set; }
        public required string Name { get; set; }
        public int CarModelId { get; set; }
        public CarModel? CarModel { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
