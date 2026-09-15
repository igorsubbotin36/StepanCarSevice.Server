using Finbuckle.MultiTenant;

namespace StepanCarService.Common.Core.Entities
{
    public class TenantInfoEntity : TenantInfo
    {
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public string ApiKey { get; set; }
        // Id пользователя портала (TenantOwner) в Auth; null — тенант создан GodMode
        public int? OwnerUserId { get; set; }
    }
}
