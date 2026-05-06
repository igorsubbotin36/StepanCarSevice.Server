using StepanCarService.Core.Models;

namespace StepanCarSevice.VisitService.Application.Interfaces
{
    public interface ITService<T> where T : class
    {
        Task<Result> AddASync(T model);
    }
}
