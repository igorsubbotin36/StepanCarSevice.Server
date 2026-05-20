
using StepanCarService.Common.Application.Models;

namespace StepanCarSevice.DetailService.Application.Interfaces
{
    public interface ITService<T> where T : class
    {
        Task<Result> AddASync(T model);
    }
}
