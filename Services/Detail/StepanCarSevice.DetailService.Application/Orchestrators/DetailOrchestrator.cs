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
    /*public class DetailOrchestrator : IDetailOrchestrator
    {
        private readonly IDetailManufactureService _detailManufactureService;
        private readonly IDetailService _detailService;
        private readonly ICarModelService _carModelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DetailOrchestrator> _logger;
        public DetailOrchestrator(IDetailManufactureService detailManufactureService,
            IDetailService detailService,
            ICarModelService carModelService,
            IUnitOfWork unitOfWork,
            ILogger<DetailOrchestrator> logger)
        {
            _detailManufactureService = detailManufactureService;
            _detailService = detailService;
            _carModelService = carModelService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<DetailReadDto>> AddAsync(DetailCreateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var detailManufactureResult = await _detailManufactureService.GetByNameOrCreateAsync(model.DetailManufacture);
                if (!detailManufactureResult.IsSuccess)
                    return Result.Failure<DetailReadDto>(detailManufactureResult.ErrorCode);
                var carModelResult = await _carModelService.GetByNameOrCreateAsync(model.CarModel, detailManufactureResult.Value.Id);
                if (!carModelResult.IsSuccess)
                    return Result.Failure<DetailReadDto>(carModelResult.ErrorCode);
                var creationResult = await _detailService.AddAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{creationResult.Value.Id} транзакция добавления завершена");
                return creationResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции добавления. Откат\n{ex}");
                return Result.Failure<DetailReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<DetailReadDto>> UpdateAsync(DetailUpdateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var detailManufactureResult = await _detailManufactureService.UpdateAsync(model.DetailManufacture);
                if (!detailManufactureResult.IsSuccess)
                    return Result.Failure<DetailReadDto>(detailManufactureResult.ErrorCode);
                var carModelResult = await _carModelService.UpdateAsync(model.CarModel, detailManufactureResult.Value.Id);
                if (!carModelResult.IsSuccess)
                    return Result.Failure<DetailReadDto>(carModelResult.ErrorCode);
                var creationResult = await _detailService.UpdateAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{creationResult.Value.Id} транзакция добавления завершена");
                return creationResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции добавления. Откат\n{ex}");
                return Result.Failure<DetailReadDto>(SystemErrors.DatabaseError);
            }
        }
    }*/
}
