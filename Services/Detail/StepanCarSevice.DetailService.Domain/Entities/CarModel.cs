using StepanCarService.Common.Core.Entities;
namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarModel : AppBaseEntity
    {
        public int ManufactureId { get; set; }
        public required CarManufacture Manufacture { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }

        public List<CarModification> CarModifications { get; set; } = new List<CarModification>();
    }
}
