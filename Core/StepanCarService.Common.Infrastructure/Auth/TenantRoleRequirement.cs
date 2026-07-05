using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.Auth
{
    public class TenantRoleRequirement : IAuthorizationRequirement
    {
        public string[] Roles { get; }
        public TenantRoleRequirement(params string[] roles) => Roles = roles;
    }
}
