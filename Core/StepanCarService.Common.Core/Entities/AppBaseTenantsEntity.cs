using System.ComponentModel.DataAnnotations;

namespace StepanCarService.Common.Core.Entities
{
    abstract public class AppBaseTenantsEntity : AppCommonEntity
    {
        public string TenantId { get; set; }
        public TenantInfoEntity Tenant { get; set; }
    }
}
