using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Core.Events
{
    public class TenantRegisteredEvent : TenantInfo
    {
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public string ApiKey { get; set; }
    }
}
