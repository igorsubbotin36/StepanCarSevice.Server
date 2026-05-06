using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class VisitDetails
    {
        [Key]
        public int Id { get; set; }
        public int VisitId { get; set; }
        public Visit Visit { get; set; }
        public int DetailId { get; set; }
        public int DetailSnapshotId { get; set; }
        public DetailSnapshot DetailSnapshot { get; set; }
    }
}
