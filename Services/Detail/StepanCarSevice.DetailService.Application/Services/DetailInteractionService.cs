using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using System.Reflection;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class DetailInteractionService : IDetailService
    {
        private readonly ICarManufactureRepository _carManufactureRepository;
        private readonly ICarModelRepository _carModelRepository;
        private readonly IDetailManufactureRepository _detailManufactureRepository;
        private readonly IDetailRepository _detailRepository;
        private readonly ILogger<DetailInteractionService> _logger;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        private readonly IUnitOfWork _unitOfWork;
        public DetailInteractionService(IDetailRepository repository,
            ILogger<DetailInteractionService> logger,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            IUnitOfWork unitOfWork,
            ICarModelRepository carModelRepository,
            IDetailManufactureRepository detailManufactureRepository,
            ICarManufactureRepository carManufactureRepository)
        {
            _detailRepository = repository;
            _logger = logger;
            _accessor = accessor;
            _unitOfWork = unitOfWork;
            _carModelRepository = carModelRepository;
            _detailManufactureRepository = detailManufactureRepository;
            _carManufactureRepository = carManufactureRepository;
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
            var tenantId = GetTenantId();
            var detail = await _detailRepository.GetByIdAsync(detailDto.Id, tenantId);
            if (detail == null)
                return Result.Failure(ModelErrors.ModelNotFound);
            if (detailDto.Count != null)
                detail.Count = (int)detailDto.Count;
            if (detailDto.DetailManufacture != null)
            {
                var detailManufacturesList = await _detailManufactureRepository.GetByNameAsync(detailDto.DetailManufacture.Name, tenantId);
                DetailManufacture detailManufacture;
                if (detailManufacturesList == null || detailManufacturesList.Count == 0)
                {
                    detailManufacture = new DetailManufacture() { Name = detailDto.DetailManufacture.Name, TenantId = tenantId };
                    _detailManufactureRepository.Add(detailManufacture);
                }
                else
                {
                    detailManufacture = detailManufacturesList.FirstOrDefault();
                }

                detail.DetailManufacture = detailManufacture;
            }
            if (detailDto.Code != null)
                detail.Code = detailDto.Code;
            if (detailDto.OriginalCode != null)
                detail.OriginalCode = detailDto.OriginalCode;
            if (detailDto.Name != null)
                detail.Name = detailDto.Name;
            if (detailDto.Price != null)
                detail.Price = (decimal)detailDto.Price;
            if (detailDto.CarModel != null) // ToDo: List<CarMocelId>
            {
                var carManufacturesList = await _carManufactureRepository.GetByNameAsync(detailDto.CarModel.Manufacture.NameEng, tenantId);
                CarManufacture carManufacture;
                if (carManufacturesList != null || carManufacturesList.Count == 0)
                {
                    carManufacture = new CarManufacture()
                    {
                        NameEN = detailDto.CarModel.Manufacture.NameEng,
                        NameRU = detailDto.CarModel.Manufacture.NameEng,
                        Country = "default" // ToDo: remove country
                    };
                    _carManufactureRepository.Add(carManufacture);
                }
                else
                {
                    carManufacture = carManufacturesList.FirstOrDefault();
                }

                var carModelsList = await _carModelRepository.GetByNameAsync(detailDto.CarModel.NameEng, tenantId);
                CarModel carModel;
                if (carModelsList != null || carModelsList.Count == 0)
                {
                    carModel = new CarModel()
                    {
                        Manufacture = carManufacture,
                        NameEN = detailDto.CarModel.NameEng,
                        NameRU = detailDto.CarModel.NameEng,
                        YearFrom = (int)detailDto.CarModel.YearFrom,
                        YearTo = (int)detailDto.CarModel.YearTo
                    };
                    detail.CarModel = carModel;
                }
            }
            try
            {
                _detailRepository.Update(detail);
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
                List<Detail> list = await _detailRepository.GetAllAsync(GetTenantId());
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
                Detail? detail = await _detailRepository.GetByIdAsync(id, GetTenantId());
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
            var tenantId = GetTenantId();
            var temp = await _detailRepository.GetDetailsByCodeAsync(model.Code, tenantId);
            if (temp.Count != 0)
                return Result.Failure<DetailReadDto>(ModelErrors.ModelAlreadyExists);

            var detailManufacturesList = await _detailManufactureRepository.GetByNameAsync(model.DetailManufacture.Name, tenantId);
            DetailManufacture detailManufacture;
            if (detailManufacturesList == null || detailManufacturesList.Count == 0)
            {
                detailManufacture = new DetailManufacture() { Name = model.DetailManufacture.Name, TenantId = tenantId };
                _detailManufactureRepository.Add(detailManufacture);
            }
            else
            {
                detailManufacture = detailManufacturesList.FirstOrDefault();
            }

            var carManufacturesList = await _carManufactureRepository.GetByNameAsync(model.CarModel.Manufacture.NameEng, tenantId);
            CarManufacture carManufacture;
            if (carManufacturesList != null || carManufacturesList.Count == 0)
            {
                carManufacture = new CarManufacture()
                {
                    NameEN = model.CarModel.Manufacture.NameEng,
                    NameRU = model.CarModel.Manufacture.NameEng,
                    Country = "default" // ToDo: remove country
                };
                _carManufactureRepository.Add(carManufacture);
            }
            else
            {
                carManufacture = carManufacturesList.FirstOrDefault();
            }

            var carModelsList = await _carModelRepository.GetByNameAsync(model.CarModel.NameEng, tenantId);
            CarModel carModel;
            if (carModelsList != null || carModelsList.Count == 0)
            {
                carModel = new CarModel()
                {
                    Manufacture = carManufacture,
                    NameEN = model.CarModel.NameEng,
                    NameRU = model.CarModel.NameEng,
                    YearFrom = model.CarModel.YearFrom,
                    YearTo = model.CarModel.YearTo
                };
            }
            else
            {
                carModel = carModelsList.FirstOrDefault();
            }

            Detail detail = new Detail()
            {
                Code = model.Code,
                OriginalCode = model.OriginalCode,
                DetailManufacture = detailManufacture,
                CarModel = carModel,
                Price = model.Price,
                Name = model.Name,
                Count = model.Count,
                TenantId = tenantId
            };
            try
            {
                _detailRepository.Add(detail);
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
