using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class CarModelSnapshot
    {
        [Key]
        public int Id { get; set; }
        public int ManufactureId { get; set; }
        public required int ManufactureSnapshotId { get; set; }
        public required ManufactureSnapshot ManufactureSnapshot { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public int YearFrom { get; set; }
        public int YearTo { get; set; }

    }
}
