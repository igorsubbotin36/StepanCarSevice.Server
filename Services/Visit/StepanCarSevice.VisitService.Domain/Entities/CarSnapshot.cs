using StepanCarService.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class CarSnapshot
    {
        [Key]
        public int Id { get; set; }
        public required int OwnerId { get; set; }
        public required int OwnerSnapshotId { get; set; }
        public required OwnerSnapshot OwnerSnapshot { get; set; }
        public required string VIN { get; set; }
        public required int CarModelId { get; set; }
        public required int CarModelSnapshotId { get; set; }
        public required CarModelSnapshot CarModelSnapshot { get; set; }
        public required int Year { get; set; }
        public required string Number { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
