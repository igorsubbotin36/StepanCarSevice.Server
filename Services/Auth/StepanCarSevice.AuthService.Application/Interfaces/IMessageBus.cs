using StepanCarService.Core.Models;

namespace StepanCarSevice.AuthService.Application.Interfaces
{
    public interface IMessageBus
    {
        Task<Result> PublishAsync<T>(T message) where T : class;
    }
}
