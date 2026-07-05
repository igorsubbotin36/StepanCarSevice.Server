using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;
        public TService(IRepository<T> repository,
            ILogger<TService<T>> logger,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> AddASync(T model)
        {
            if (model == null)
                return Result.Failure<T>(ModelErrors.ModelNotFound);
            try
            {
                await _repository.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();
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
