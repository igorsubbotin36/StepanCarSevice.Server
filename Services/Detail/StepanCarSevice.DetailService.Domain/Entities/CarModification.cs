using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class CarModification : AppBaseEntity
    {
        public int CarModelId { get; set; }
        public CarModel CarModel { get; set; }
        public string? EnginePower { get; set; }
        public int WheelDriveTypeId { get; set; }
        public WheelDriveType WheelDriveType { get; set; }
        public int TransmissionTypeId { get; set; }
        public TransmissionType TransmissionType { get; set; }

        public List<Engine> Engines { get; set; } = new List<Engine>();
        public List<Detail> Details { get; set; } = new List<Detail>();
    }
}
