using Finbuckle.MultiTenant;

namespace StepanCarSevice.TenantService.Core.Entities;

public class TenantInfoEntity : TenantInfo
{
    public string ConnectionString { get; set; }
    public bool IsActive { get; set; }
    public string ApiKey { get; set; }
}