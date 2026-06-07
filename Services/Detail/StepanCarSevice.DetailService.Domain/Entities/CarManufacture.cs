using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarManufacture : AppBaseEntity
    {
        public List<CarModel> CarModels { get; set; } = new List<CarModel>();
    }
}
