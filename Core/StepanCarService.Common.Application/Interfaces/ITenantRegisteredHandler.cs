using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Application.Interfaces
{
    public interface ITenantRegisteredHandler
    {
        Task<Result> HandleAsync(TenantEvent tenantEvent);
    }
}
