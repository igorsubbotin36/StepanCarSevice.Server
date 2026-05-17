using Microsoft.Extensions.Logging;
using StepanCarService.Core.Models;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class DetailInteractionService : IDetailService
    {
        private readonly IDetailRepository _detailRepository;
        private readonly ILogger<DetailInteractionService> _logger;
        public DetailInteractionService(IDetailRepository repository,
            ILogger<DetailInteractionService> logger)
        {
            _detailRepository = repository;
            _logger = logger;
        }
        public async Task<Result> EditDetailAsync(Detail detail)
        {
            if (detail == null)
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            try
            {
                await _detailRepository.EditDetailAsync(detail);
                _logger.LogInformation($"{detail.Id} обновлена информация в БД");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{detail.Id} ошибка БД при попытке обновления информации о детали\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<Detail>>> GetAllDetailsAsync()
        {
            List<Detail> list = await _detailRepository.GetAllDetailsAsync();
            if (list == null || list.Count == 0)
                return Result.Failure<List<Detail>>(ModelErrors.ModelNotFound);
            return Result.Success(list);
        }

        public async Task<Result<Detail>> GetDetailByIdAsync(int id)
        {
            Detail? detail = await _detailRepository.GetDetailByIdAsync(id);
            if (detail == null)
                return Result.Failure<Detail>(ModelErrors.ModelNotFound);
            return Result.Success(detail);
        }

        public async Task<Result<List<Detail>>> GetDetailsByCodeAsync(string code)
        {
            List<Detail> list = await _detailRepository.GetDetailsByCodeAsync(code);
            if (list == null || list.Count == 0)
                return Result.Failure<List<Detail>>(ModelErrors.ModelNotFound);
            return Result.Success(list);
        }
    }
}
