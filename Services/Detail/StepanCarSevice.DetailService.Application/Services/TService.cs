using StepanCarService.Core.Models;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class TService<T> : ITService<T> where T : class
    {
        private readonly IRepository<T> _repository;
        public TService(IRepository<T> repository)
        {
            _repository = repository;
        }
        public async Task<Result> AddASync(T model)
        {
            if (model == null)
                return Result.Failure<T>(ModelErrors.ModelNotFound);
            bool addSuccess = await _repository.AddAsync(model);
            if (!addSuccess)
                return Result.Failure(SystemErrors.DatabaseError);
            return Result.Success();
        }
    }
}
