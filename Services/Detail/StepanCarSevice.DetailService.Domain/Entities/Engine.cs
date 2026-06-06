using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Engine : AppBaseEntity
    {
        public string? EngineValue { get; set; }
        public int EngineTypeId { get; set; }
        public EngineType EngineType { get; set; }
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }

        public List<CarModification> CarModifications { get; set; } = new List<CarModification>();
    }
}
