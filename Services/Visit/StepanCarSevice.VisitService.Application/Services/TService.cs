using Microsoft.Extensions.Logging;
using StepanCarService.Core.Models;
using StepanCarSevice.VisitService.Application.Interfaces;
using StepanCarSevice.VisitService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.VisitService.Application.Services
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
                _logger.LogInformation($"{model.ToString} добавлена в БД");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.ToString} ошибка при добавлении в БД");
                return Result.Failure(SystemErrors.DatabaseError);
            }
                
            
        }
    }
}
