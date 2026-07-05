using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Application.Models.Mappers;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class CarModificationService : ServiceBase, ICarModificationService
    {
        private readonly ICarModificationRepository _carModificationRepository;
        private readonly ILogger<CarModificationService> _logger;
        private readonly ICarModificationMappers _carModificationMapper;
        private readonly ICommonRepository<WheelDriveType> _wheelDriveTypeRepository;
        private readonly ICommonRepository<TransmissionType> _transmissionTypeRepository;
        private readonly ICommonRepository<EngineType> _engineTypeRepository;
        private readonly ICommonRepository<Engine> _engineRepository;
        public CarModificationService(IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ICarModificationRepository repository,
            ILogger<CarModificationService> logger,
            ICarModificationMappers carModificationMappers,
            ICommonRepository<WheelDriveType> wheelDriveTypeRepository,
            ICommonRepository<TransmissionType> transmissionTypeRepository,
            ICommonRepository<EngineType> engineTypeRepository,
            ICommonRepository<Engine> engineRepository) : base(accessor) 
        {
            _carModificationRepository = repository;
            _logger = logger;
            _carModificationMapper = carModificationMappers;
            _wheelDriveTypeRepository = wheelDriveTypeRepository;
            _transmissionTypeRepository = transmissionTypeRepository;
            _engineTypeRepository = engineTypeRepository;
            _engineRepository = engineRepository;
        }
        public async Task<Result<CarModificationReadDto>> AddAsync(CarModificationCreateDto model)
        {
            try
            {
                if (model == null)
                    return Result.Failure<CarModificationReadDto>(ModelErrors.RequestedModelIsNull);
                var tenantId = GetTenantId();
                var temp = await _carModificationRepository.GetByNameAsync(model.Name, tenantId);
                if (temp.Count != 0)
                    return Result.Failure<CarModificationReadDto>(ModelErrors.ModelAlreadyExists);

                var enginesList = await _engineRepository.GetByNameAsync(model.EngineName);
                var engine = enginesList.SingleOrDefault(x => x.CarModelId == model.CarModelId);
                if (engine == null)
                {
                    engine = new Engine
                    {
                        Name = model.EngineName,
                        CarModelId = model.CarModelId,
                        EngineTypeId = model.EngineTypeId,
                        EngineValue = model.EngineValue
                    };
                    await _engineRepository.AddAsync(engine);
                }
                var resultEntity = await _carModificationRepository.AddAsync(_carModificationMapper.CreateDtoToCarModification(model));
                resultEntity.Engines.Add(engine);
                return Result.Success(_carModificationMapper.CarModificationToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Name} ошибка добавления\n{ex}");
                return Result.Failure<CarModificationReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result> DeleteByIdAsync(int Id)
        {
            try
            {
                var tenantId = GetTenantId();
                var entity = await _carModificationRepository.GetByIdAsync(Id, tenantId);
                _carModificationRepository.Delete(entity);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления с Id {Id}\n{ex}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarModificationReadDto>>> GetAllAsync()
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carModificationRepository.GetAllAsync(tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarModificationReadDto>>(EntityErrors.EntityNotFound);
                List<CarModificationReadDto> resultList = new List<CarModificationReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carModificationMapper.CarModificationToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка\n{ex}");
                return Result.Failure<List<CarModificationReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<List<CarModificationReadDto>>> GetAllModificationsByModelId(int modelId)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntities = await _carModificationRepository.GetAllModificationsByModelId(modelId, tenantId);
                if (resultEntities == null || resultEntities.Count == 0)
                    return Result.Failure<List<CarModificationReadDto>>(EntityErrors.EntityNotFound);
                List<CarModificationReadDto> resultList = new List<CarModificationReadDto>();
                foreach (var entity in resultEntities)
                    resultList.Add(_carModificationMapper.CarModificationToReadDto(entity));
                return Result.Success(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка\n{ex}");
                return Result.Failure<List<CarModificationReadDto>>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModificationReadDto>> GetById(int id)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carModificationRepository.GetByIdAsync(id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarModificationReadDto>(EntityErrors.EntityNotFound);
                return Result.Success(_carModificationMapper.CarModificationToReadDto(resultEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении Id {id}");
                return Result.Failure<CarModificationReadDto>(SystemErrors.DatabaseError);
            }
        }

        public async Task<Result<CarModificationReadDto>> UpdateAsync(CarModificationUpdateDto model)
        {
            try
            {
                var tenantId = GetTenantId();
                var resultEntity = await _carModificationRepository.GetByIdAsync(model.Id, tenantId);
                if (resultEntity == null)
                    return Result.Failure<CarModificationReadDto>(EntityErrors.EntityNotFound);

                if (model.Name != null || model.Name != resultEntity.Name)
                    resultEntity.Name = model.Name;
                if (model.TransmissionTypeId != null || model.TransmissionTypeId != resultEntity.TransmissionTypeId)
                    resultEntity.TransmissionTypeId = (int)model.TransmissionTypeId;
                if (model.WheelDriveTypeId != null || model.WheelDriveTypeId != resultEntity.WheelDriveTypeId)
                    resultEntity.WheelDriveTypeId = (int)model.WheelDriveTypeId;
                if (model.CarModelId != null && model.CarModelId != resultEntity.CarModelId)
                    resultEntity.CarModelId = (int)model.CarModelId;
                if (model.EngineName != null)
                {
                    var enginesList = await _engineRepository.GetByNameAsync(model.EngineName);
                    var engine = enginesList.SingleOrDefault(x => x.CarModelId == model.CarModelId);
                    if (engine == null)
                    {
                        engine = new Engine
                        {
                            Name = model.EngineName,
                            CarModelId = (int)model.CarModelId,
                            EngineTypeId = (int)model.EngineTypeId,
                            EngineValue = model.EngineValue
                        };
                        await _engineRepository.AddAsync(engine);
                        resultEntity.Engines.Add(engine);          //????????
                    }
                    else if (!engine.CarModifications.Any(x => x.Id == resultEntity.Id))
                    {
                        engine.CarModifications.Add(resultEntity);
                        resultEntity.Engines.Add(engine);
                    }
                }
                var result = _carModificationRepository.Update(resultEntity);
                return Result.Success(_carModificationMapper.CarModificationToReadDto(result));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{model.Id} ошибка обновления\n{ex}");
                return Result.Failure<CarModificationReadDto>(SystemErrors.DatabaseError);
            }
        }
    }
}
