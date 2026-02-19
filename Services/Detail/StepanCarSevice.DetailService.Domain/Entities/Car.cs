using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public required int OwnerId { get; set; }
        public required User Owner { get; set; }
        public required string WIN { get; set; }
        public required int CarModelId { get; set; }
        public required CarModel CarModel { get; set; }
        public required int Year { get; set; }
        public required string Number { get; set; }
    }
}
