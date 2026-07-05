using StepanCarService.Common.Core.Entities;

namespace StepanCarService.TenantService.Application.Models.DTOs;

public record TenantDto(string Id, string Identifier, string Name, string ConnectionString, bool IsActive, string ApiKey)
{
    public static TenantDto FromTenant(TenantInfoEntity t) => new TenantDto(t.Id, 
        t.Identifier, 
        t.Name, 
        t.ConnectionString, 
        t.IsActive, 
        t.ApiKey
        );
}