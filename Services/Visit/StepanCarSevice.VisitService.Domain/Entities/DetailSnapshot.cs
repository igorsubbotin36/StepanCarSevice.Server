using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class DetailSnapshot
    {
        [Key]
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required int CarModelId { get; set; }
        public required int CarModelSnapshotId { get; set; }
        public required CarModelSnapshot CarModelSnapshot { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
