using StepanCarService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.TenantService.Application.Interfaces
{
    public interface IMessageBus
    {
        Task<Result> PublishAsync<T>(T message) where T : class;
    }
}
