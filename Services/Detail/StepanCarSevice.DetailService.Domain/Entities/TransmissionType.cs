using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class TransmissionType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
