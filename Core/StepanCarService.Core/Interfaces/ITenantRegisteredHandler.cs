using StepanCarService.Core.Events;
using StepanCarService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Interfaces
{
    public interface ITenantRegisteredHandler
    {
        Task<Result> HandleAsync(TenantEvent tenantEvent);
    }
}
