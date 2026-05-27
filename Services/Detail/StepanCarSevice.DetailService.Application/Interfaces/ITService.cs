
using StepanCarService.Common.Application.Models;

namespace StepanCarSevice.DetailService.Application.Interfaces
{
    public interface ITService<T, D> where T : class
    {
        Task<Result<D>> AddASync(T model);
    }
}
