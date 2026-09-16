using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using StepanCarService.Common.Core.Entities;

namespace StepanCarService.TestKit.MultiTenancy;

// Тенант «текущего запроса» для тестов без HTTP. Tenant = null — портал или фоновая задача.
// Тенант можно переключать между действиями, чтобы проверить изоляцию в одном контексте
public sealed class FakeTenantAccessor : IMultiTenantContextAccessor<TenantInfoEntity>
{
    public FakeTenantAccessor(TenantInfoEntity? tenant = null)
    {
        Tenant = tenant;
    }

    public TenantInfoEntity? Tenant { get; set; }

    public IMultiTenantContext<TenantInfoEntity> MultiTenantContext =>
        new MultiTenantContext<TenantInfoEntity> { TenantInfo = Tenant };

    IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;
}
