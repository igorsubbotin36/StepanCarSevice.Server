namespace StepanCarService.TenantService.Domain.Entities;

public class Tenant
{
    public string Id { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public bool IsActive { get; set; }
    public string ApiKey { get; set; }
}