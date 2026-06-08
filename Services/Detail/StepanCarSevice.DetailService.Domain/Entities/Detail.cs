using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Detail : AppBaseTenantsEntity
    {
        public required string Code { get; set; }
        public required string OriginalCode { get; set; }
        public int DetailManufactureId { get; set; }
        public DetailManufacture? DetailManufacture { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }

        public List<CarModification> CarModifications { get; set; } = new List<CarModification>();
        public List<Detail> OriginalDetails { get; set; } = new List<Detail>();
        public List<Detail> AlternativeDetails { get; set; } = new List<Detail>();
    }
}
