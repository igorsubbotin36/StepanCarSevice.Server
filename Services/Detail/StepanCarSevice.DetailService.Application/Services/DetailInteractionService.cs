using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class DetailInteractionService : IDetailService
    {
        private readonly IDetailRepository _detailRepository;
        private readonly ILogger<DetailInteractionService> _logger;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        public DetailInteractionService(IDetailRepository repository,
            ILogger<DetailInteractionService> logger,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor)
        {
            _detailRepository = repository;
            _logger = logger;
            _accessor = accessor;
        }
        private TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo;
        private string? GetTenantId()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
                return null;
            return tenant.Id;
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
            try
            {
                List<Detail> list = await _detailRepository.GetAllDetailsAsync(GetTenantId());
                if (list == null || list.Count == 0)
                    return Result.Failure<List<Detail>>(ModelErrors.ModelNotFound);
                return Result.Success(list);
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения списка деталей на складе\n{e}");
                return Result.Failure<List<Detail>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<Detail>> GetDetailByIdAsync(int id)
        {
            try
            {
                Detail? detail = await _detailRepository.GetDetailByIdAsync(id, GetTenantId());
                if (detail == null)
                    return Result.Failure<Detail>(ModelErrors.ModelNotFound);
                return Result.Success(detail);
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения детали со склада\n{e}");
                return Result.Failure<Detail>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<Detail>>> GetDetailsByCodeAsync(string code)
        {
            try
            {
                List<Detail> list = await _detailRepository.GetDetailsByCodeAsync(code, GetTenantId());
                if (list == null || list.Count == 0)
                    return Result.Failure<List<Detail>>(ModelErrors.ModelNotFound);
                return Result.Success(list);
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения списка деталей по коду на складе\n{e}");
                return Result.Failure<List<Detail>>(SystemErrors.DatabaseError);
            }
        }
    }
}
