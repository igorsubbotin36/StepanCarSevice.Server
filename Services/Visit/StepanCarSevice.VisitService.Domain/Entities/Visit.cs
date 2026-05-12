using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }
        public required int CarId { get; set; }
        public required int CarSnapshotId { get; set; }
        public required CarSnapshot Car { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
        public ICollection<Work> Works { get; set; } = new List<Work>();
    }
}
