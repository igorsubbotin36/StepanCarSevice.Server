using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Engine : AppBaseTenantsEntity
    {
        public string? EngineValue { get; set; }
        public int EngineTypeId { get; set; }
        public EngineType EngineType { get; set; }
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }

        public List<CarModification> CarModifications { get; set; } = new List<CarModification>();
    }
}
