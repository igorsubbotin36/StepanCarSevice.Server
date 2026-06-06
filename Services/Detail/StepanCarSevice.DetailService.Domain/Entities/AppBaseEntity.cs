using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Entities
{
    abstract public class AppBaseEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }

    }
}
