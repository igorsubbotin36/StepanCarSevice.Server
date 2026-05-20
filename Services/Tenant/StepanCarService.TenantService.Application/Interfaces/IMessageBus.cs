using StepanCarService.Common.Application.Models;

namespace StepanCarService.TenantService.Application.Interfaces
{
    public interface IMessageBus
    {
        Task<Result> PublishAsync<T>(T message) where T : class;
    }
}
