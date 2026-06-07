using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces.Orchestrators;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Orchestrators
{
    public class CarManufactureOrchestrator : ICarManufactureOrchestrator
    {
        private readonly ICarManufactureService _carManufactureService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CarManufactureOrchestrator> _logger;
        public CarManufactureOrchestrator(ICarManufactureService carManufactureService,
            IUnitOfWork unitOfWork,
            ILogger<CarManufactureOrchestrator> logger)
        {
            _carManufactureService = carManufactureService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<CarManufactureReadDto>> AddAsync(CarManufactureCreateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var creationResult = await _carManufactureService.AddAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{creationResult.Value.Name} транзакция добавления завершена");
                return creationResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.NameEng} ошибка при сохранении транзакции добавления. Откат\n{ex}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarManufactureReadDto>> UpdateAsync(CarManufactureUpdateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var updateResult = await _carManufactureService.UpdateAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{updateResult.Value.Name} транзакция обновления завершена");
                return Result.Success(updateResult.Value);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции обновления. Откат\n{ex}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
