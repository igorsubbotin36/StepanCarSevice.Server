using StepanCarService.Common.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class VisitDetails : ITenantScoped
    {
        [Key]
        public int Id { get; set; }
        public int VisitId { get; set; }
        public Visit Visit { get; set; }
        public int DetailId { get; set; }
        public int DetailSnapshotId { get; set; }
        public DetailSnapshot DetailSnapshot { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
