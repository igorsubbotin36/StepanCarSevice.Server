using Finbuckle.MultiTenant.Abstractions;
using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Application.Services
{
    abstract public class ServiceBase
    {
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        protected ServiceBase(IMultiTenantContextAccessor<TenantInfoEntity> accessor)
        {
            _accessor = accessor;
        }

        protected TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo; 

        protected string? GetTenantId()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
                return null;
            return tenant.Id;
        }
    }
}
