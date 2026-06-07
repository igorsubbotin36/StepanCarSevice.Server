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
    /*public class CarModelOrchestrator : ICarModelOrchestrator
    {
        private readonly ICarManufactureService _carManufactureService;
        private readonly ICarModelService _carModelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CarModelOrchestrator> _logger;
        public CarModelOrchestrator(ICarManufactureService carManufactureService,
            ICarModelService carModelService,
            IUnitOfWork unitOfWork,
            ILogger<CarModelOrchestrator> logger)
        {
            _carManufactureService = carManufactureService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _carModelService = carModelService;
        }
        public async Task<Result<CarModelReadDto>> AddAsync(CarModelCreateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var carManufactureResult = await _carManufactureService.GetByNameOrCreateAsync(model.Manufacture);
                if (!carManufactureResult.IsSuccess)
                    return Result.Failure<CarModelReadDto>(carManufactureResult.ErrorCode);
                var carModelResult = await _carModelService.AddAsync(model, carManufactureResult.Value.Id);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{carModelResult.Value.Id} транзакция добавления завершена");
                return carModelResult;
            }
            catch (Exception ex) 
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции добавления. Откат\n{ex}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModelReadDto>> UpdateAsync(CarModelUpdateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var carManufactureResult = await _carManufactureService.UpdateAsync(model.Manufacture);
                if (!carManufactureResult.IsSuccess)
                    return Result.Failure<CarModelReadDto>(carManufactureResult.ErrorCode);
                var carModelResult = await _carModelService.UpdateAsync(model, carManufactureResult.Value.Id);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{carModelResult.Value.Id} транзакция обновления завершена");
                return Result.Success<CarModelReadDto>(carModelResult.Value);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции обновления. Откат\n{ex}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }
    }*/
}
