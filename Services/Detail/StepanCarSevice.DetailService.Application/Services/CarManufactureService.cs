using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class CarManufactureService : ServiceBase, ICarManufactureService
    {
        private readonly IRepository<CarManufacture> _carManufactureRepository;
        private readonly ILogger<CarManufactureService> _logger;
        private readonly ICarManufactureMappers _carManufactureMapper;
        public CarManufactureService(IRepository<CarManufacture> carManufactureRepository,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ILogger<CarManufactureService> logger,
            ICarManufactureMappers carManufactureMapper) : base(accessor)
        {
            _carManufactureRepository = carManufactureRepository;
            _logger = logger;
            _carManufactureMapper = carManufactureMapper;
        }

        public async Task<Result<CarManufactureReadDto>> AddAsync(CarManufactureCreateDto model)
        {
            try
            {
                if (model == null)
                    return Result.Failure<CarManufactureReadDto>(ModelErrors.RequestedModelIsNull);
                var tenantId = GetTenantId();
                var temp = await _carManufactureRepository.GetByNameAsync(model.NameEng, tenantId);
                if (temp.Count != 0)
                    return Result.Failure<CarManufactureReadDto>(ModelErrors.ModelAlreadyExists);
                var resultEntity = await _carManufactureRepository.AddAsync(_carManufactureMapper.CreateDtoToCarManufacture(model));
                return Result.Success(_carManufactureMapper.CarManufactureToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.NameEng} ошибка добавления\n{ex}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result> DeleteByIdAsync(int Id)
        {
            try
            {
                var tenantId = GetTenantId();
                var entity = await _carManufactureRepository.GetByIdAsync(Id, tenantId);
                _carManufactureRepository.Delete(entity);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления с Id {Id}\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarManufactureReadDto>>> GetAllAsync()
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carManufactureRepository.GetAllAsync(tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarManufactureReadDto>>(EntityErrors.EntityNotFound);
                List<CarManufactureReadDto> resultList = new List<CarManufactureReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carManufactureMapper.CarManufactureToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка\n{ex}");
                return Result.Failure<List<CarManufactureReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarManufactureReadDto>> GetById(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carManufactureRepository.GetByIdAsync(id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarManufactureReadDto>(EntityErrors.EntityNotFound);
                return Result.Success(_carManufactureMapper.CarManufactureToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении Id {id}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarManufactureReadDto>>> GetByNameAsync(string name)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carManufactureRepository.GetByNameAsync(name, tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarManufactureReadDto>>(EntityErrors.EntityNotFound);
                List<CarManufactureReadDto> resultList = new List<CarManufactureReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carManufactureMapper.CarManufactureToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении Name {name}");
                return Result.Failure<List<CarManufactureReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarManufactureReadDto>> GetByNameOrCreateAsync(CarManufactureCreateDto model)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carManufactureRepository.GetByNameAsync(model.NameEng, tenantId);
                Result<CarManufactureReadDto> result;
                if (resultEntities == null || resultEntities.Count == 0)
                    return await AddAsync(model);
                return Result.Success(_carManufactureMapper.CarManufactureToReadDto(resultEntities.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.NameEng} ошибка при получении/создании\n{ex}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarManufactureReadDto>> UpdateAsync(CarManufactureUpdateDto model)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carManufactureRepository.GetByIdAsync(model.Id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarManufactureReadDto>(EntityErrors.EntityNotFound);
                if (model.Name != null || model.Name != resultEntity.Name)
                    resultEntity.Name = model.Name;
                var result = _carManufactureRepository.Update(resultEntity);
                return Result.Success(_carManufactureMapper.CarManufactureToReadDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Id} ошибка обновления\n{ex}");
                return Result.Failure<CarManufactureReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
