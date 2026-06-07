using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class DetailManufacture : AppBaseEntity
    {
        public List<Detail> Details { get; set; } = new List<Detail>();
    }
}
