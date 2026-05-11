using Finbuckle.MultiTenant;

namespace StepanCarService.Core.Entities
{
    public class TenantInfoEntity : TenantInfo
    {
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public string ApiKey { get; set; }
    }
}
