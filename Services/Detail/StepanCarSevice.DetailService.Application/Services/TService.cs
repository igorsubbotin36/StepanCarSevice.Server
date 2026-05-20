using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class TService<T> : ITService<T> where T : class
    {
        private readonly IRepository<T> _repository;
        private readonly ILogger<TService<T>> _logger;
        public TService(IRepository<T> repository,
            ILogger<TService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<Result> AddASync(T model)
        {
            if (model == null)
                return Result.Failure<T>(ModelErrors.ModelNotFound);
            try
            {
                await _repository.AddAsync(model);
                _logger.LogInformation($"{model.ToString} добавлена в базу");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.ToString} ошибка при добавлении в базу\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }
    }
}
