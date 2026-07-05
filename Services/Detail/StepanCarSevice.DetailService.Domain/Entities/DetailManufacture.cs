using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class DetailManufacture : AppBaseTenantsEntity
    {
        public List<Detail> Details { get; set; } = new List<Detail>();
    }
}
