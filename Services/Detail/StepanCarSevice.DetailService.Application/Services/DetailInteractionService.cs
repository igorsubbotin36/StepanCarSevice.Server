using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class DetailInteractionService : IDetailService
    {
        private readonly IDetailRepository _detailRepository;
        private readonly ILogger<DetailInteractionService> _logger;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        private readonly IUnitOfWork _unitOfWork;
        public DetailInteractionService(IDetailRepository repository,
            ILogger<DetailInteractionService> logger,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            IUnitOfWork unitOfWork)
        {
            _detailRepository = repository;
            _logger = logger;
            _accessor = accessor;
            _unitOfWork = unitOfWork;
        }
        private TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo;
        private string? GetTenantId()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
                return null;
            return tenant.Id;
        }
        public async Task<Result> EditDetailAsync(DetailUpdateDto detailDto)
        {
            if (detailDto == null)
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            var detail = await _detailRepository.GetDetailByIdAsync(detailDto.Id, GetTenantId());
            if (detail == null)
                return Result.Failure(ModelErrors.ModelNotFound);
            if (detailDto.Count != null)
                detail.Count = (int)detailDto.Count;
            if (detailDto.DetailManufactureId != null)
                detail.DetailManufactureId = (int)detailDto.DetailManufactureId;
            if (detailDto.Code != null)
                detail.Code = detailDto.Code;
            if (detailDto.OriginalCode != null)
                detail.OriginalCode = detailDto.OriginalCode;
            if (detailDto.Name != null)
                detail.Name = detailDto.Name;
            if (detailDto.Price != null)
                detail.Price = (decimal)detailDto.Price;
            if (detailDto.CarModelId != null) // ToDo: List<CarMocelId>
                detail.CarModelId = (int)detailDto.CarModelId;
            try
            {
                await _detailRepository.EditDetailAsync(detail);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"{detail.Id} обновлена информация в БД");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{detail.Id} ошибка БД при попытке обновления информации о детали\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<DetailReadDto>>> GetAllDetailsAsync()
        {
            try
            {
                List<Detail> list = await _detailRepository.GetAllDetailsAsync(GetTenantId());
                if (list == null || list.Count == 0)
                    return Result.Failure<List<DetailReadDto>>(ModelErrors.ModelNotFound);
                List<DetailReadDto> result = new List<DetailReadDto>();
                foreach (var detail in list)
                {
                    result.Add(MapDetailToReadDto(detail));
                }
                return Result.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения списка деталей на складе\n{e}");
                return Result.Failure<List<DetailReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<DetailReadDto>> GetDetailByIdAsync(int id)
        {
            try
            {
                Detail? detail = await _detailRepository.GetDetailByIdAsync(id, GetTenantId());
                if (detail == null)
                    return Result.Failure<DetailReadDto>(ModelErrors.ModelNotFound);
                return Result.Success(MapDetailToReadDto(detail));
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения детали со склада\n{e}");
                return Result.Failure<DetailReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<DetailReadDto>>> GetDetailsByCodeAsync(string code)
        {
            try
            {
                List<Detail>? list = await _detailRepository.GetDetailsByCodeAsync(code, GetTenantId());
                if (list == null || list.Count == 0)
                    return Result.Failure<List<DetailReadDto>>(ModelErrors.ModelNotFound);
                List<DetailReadDto> result = new List<DetailReadDto>();
                foreach (var detail in list)
                {
                    result.Add(MapDetailToReadDto(detail));
                }
                return Result.Success(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка получения списка деталей по коду на складе\n{e}");
                return Result.Failure<List<DetailReadDto>>(SystemErrors.DatabaseError);
            }
        }

        private DetailReadDto MapDetailToReadDto(Detail detail)
        {
            DetailReadDto readDto = new DetailReadDto(detail.Id,
                detail.Code,
                detail.OriginalCode,
                detail.DetailManufactureId,
                detail.Name,
                detail.CarModelId,
                detail.Price,
                detail.Count);
            return readDto;
        }
        public async Task<Result<DetailReadDto>> AddASync(DetailCreateDto model)
        {
            if (model == null)
                return Result.Failure<DetailReadDto>(ModelErrors.RequestedModelIsNull);
            var temp = await _detailRepository.GetDetailsByCodeAsync(model.Code, GetTenantId());
            if (temp.Count != 0)
                return Result.Failure<DetailReadDto>(ModelErrors.ModelAlreadyExists);
            Detail detail = new Detail()
            {
                Code = model.Code,
                OriginalCode = model.OriginalCode,
                DetailManufactureId = model.DetailManufactureId,
                CarModelId = model.CarModelId,
                Price = model.Price,
                Name = model.Name,
                Count = model.Count,
                TenantId = GetTenantId()
            };
            try
            {
                await _detailRepository.AddAsync(detail);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success(MapDetailToReadDto(detail));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при добавлении детали в базую\n{ex}");
                return Result.Failure<DetailReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
