using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Application.Models.Mappers;
using StepanCarSevice.DetailService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class CarModelService : ServiceBase, ICarModelService
    {
        private readonly ICarModelRepository _carModelRepository;
        private readonly ICarModelMappers _carModelMappers;
        private readonly ILogger<CarModelService> _logger;
        public CarModelService(IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ICarModelRepository carModelRepository,
            ICarModelMappers carModelMappers,
            ILogger<CarModelService> logger) : base(accessor)
        {
            _carModelRepository = carModelRepository;
            _carModelMappers = carModelMappers;
            _logger = logger;
        }
        public async Task<Result<CarModelReadDto>> AddAsync(CarModelCreateDto model)
        {
            try
            {
                if (model == null)
                    return Result.Failure<CarModelReadDto>(ModelErrors.RequestedModelIsNull);
                var tenantId = GetTenantId();
                var temp = await _carModelRepository.GetByNameAsync(model.Name, tenantId);
                if (temp.Count != 0)
                    return Result.Failure<CarModelReadDto>(ModelErrors.ModelAlreadyExists);
                var resultEntity = await _carModelRepository.AddAsync(_carModelMappers.CreateDtoToCarModel(model));
                return Result.Success(_carModelMappers.CarModelToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Name} ошибка добавления\n{ex}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result> DeleteByIdAsync(int Id)
        {
            try
            {
                var tenantId = GetTenantId();
                var entity = await _carModelRepository.GetByIdAsync(Id, tenantId);
                _carModelRepository.Delete(entity);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления с Id {Id}\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarModelReadDto>>> GetAllAsync()
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carModelRepository.GetAllAsync(tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarModelReadDto>>(EntityErrors.EntityNotFound);
                List<CarModelReadDto> resultList = new List<CarModelReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carModelMappers.CarModelToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка\n{ex}");
                return Result.Failure<List<CarModelReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModelReadDto>> GetById(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carModelRepository.GetByIdAsync(id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarModelReadDto>(EntityErrors.EntityNotFound);
                return Result.Success(_carModelMappers.CarModelToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении Id {id}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarModelReadDto>>> GetByNameAsync(string name)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carModelRepository.GetByNameAsync(name, tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarModelReadDto>>(EntityErrors.EntityNotFound);
                List<CarModelReadDto> resultList = new List<CarModelReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carModelMappers.CarModelToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении Name {name}");
                return Result.Failure<List<CarModelReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModelReadDto>> GetByNameOrCreateAsync(CarModelCreateDto model, int manufactureId)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carModelRepository.GetByNameAsync(model.Name, tenantId);
                Result<CarManufactureReadDto> result;
                if (resultEntities == null || resultEntities.Count == 0)
                    return await AddAsync(model);
                return Result.Success(_carModelMappers.CarModelToReadDto(resultEntities.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Name} ошибка при получении/создании\n{ex}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModelReadDto>> UpdateAsync(CarModelUpdateDto model)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carModelRepository.GetByIdAsync(model.Id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarModelReadDto>(EntityErrors.EntityNotFound);

                if (model.Name != null || model.Name != resultEntity.Name)
                    resultEntity.Name = model.Name;
                if (model.YearFrom != null || model.YearFrom != resultEntity.YearFrom)
                    resultEntity.YearFrom = (int)model.YearFrom;
                if (model.YearTo != null || model.YearTo != resultEntity.YearTo)
                    resultEntity.YearTo = (int)model.YearTo;
                if (model.ManufactureId != null && model.ManufactureId != resultEntity.ManufactureId)
                    resultEntity.ManufactureId = model.ManufactureId;

                var result = _carModelRepository.Update(resultEntity);
                return Result.Success(_carModelMappers.CarModelToReadDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Id} ошибка обновления\n{ex}");
                return Result.Failure<CarModelReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
