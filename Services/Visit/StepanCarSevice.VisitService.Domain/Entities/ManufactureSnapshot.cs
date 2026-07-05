using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.VisitService.Domain.Entities
{
    public class ManufactureSnapshot
    {
        [Key]
        public int Id { get; set; }
        public required string NameEN { get; set; }
        public required string NameRU { get; set; }
        public required string Country { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
