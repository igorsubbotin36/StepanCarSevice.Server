using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.Auth
{
    public class TenantRoleHandler : AuthorizationHandler<TenantRoleRequirement>
    {
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _tenantAccessor;
        public TenantRoleHandler(IMultiTenantContextAccessor<TenantInfoEntity> tenantAccessor)
        {
            _tenantAccessor = tenantAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRoleRequirement requirement)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == null || !requirement.Roles.Contains(userRole))
            {
                return Task.CompletedTask;
            }
            if (userRole == "GodMode")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            var userTenantId = context.User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrWhiteSpace(userTenantId))
            {
                return Task.CompletedTask;
            }
            var currentTenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
            if (!string.IsNullOrWhiteSpace(currentTenantId)
                && userTenantId.Equals(currentTenantId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
