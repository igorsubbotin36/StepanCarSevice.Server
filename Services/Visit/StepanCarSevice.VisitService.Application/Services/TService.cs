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
        private readonly ILogger<T> _logger;
        public TService(IRepository<T> repository,
            ILogger<T> logger)
        {
            _repository = repository;
            _logger = logger;
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
