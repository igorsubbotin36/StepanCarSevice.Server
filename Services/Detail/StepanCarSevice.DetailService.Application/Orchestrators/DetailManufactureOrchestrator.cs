using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces.Orchestrators;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Orchestrators
{
    public class DetailManufactureOrchestrator : IDetailManufactureOrchestrator
    {
        private readonly IDetailManufactureService _detailManufactureService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DetailManufactureOrchestrator> _logger;

        public DetailManufactureOrchestrator(IDetailManufactureService detailManufactureService,
            IUnitOfWork unitOfWork,
            ILogger<DetailManufactureOrchestrator> logger)
        {
            _detailManufactureService = detailManufactureService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<DetailManufactureReadDto>> AddAsync(DetailManufactureCreateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var creationResult = await _detailManufactureService.AddAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{creationResult.Value.Name} транзакция добавления завершена");
                return creationResult;
            }
            catch (Exception ex) 
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model} ошибка при сохранении транзакции добавления. Откат\n{ex}");
                return Result.Failure<DetailManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<DetailManufactureReadDto>> UpdateAsync(DetailManufactureUpdateDto model)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var updateResult = await _detailManufactureService.UpdateAsync(model);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation($"{updateResult.Value.Name} транзакция обновления завершена");
                return Result.Success(updateResult.Value);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackTransactionAsync();
                _logger.LogError($"{model.Name} ошибка при сохранении транзакции обновления. Откат\n{ex}");
                return Result.Failure<DetailManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
