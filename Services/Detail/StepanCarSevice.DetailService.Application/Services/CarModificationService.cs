using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Application.Interfaces.Mappers;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Repositories;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class CarModificationService : ServiceBase, ICarModificationService
    {
        private readonly ICarModificationRepository _carModificationRepository;
        private readonly ILogger<CarModificationService> _logger;
        private readonly ICarModificationMappers _carModificationMapper;
        public CarModificationService(IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ICarModificationRepository repository,
            ILogger<CarModificationService> logger,
            ICarModificationMappers carModificationMappers) : base(accessor) 
        {
            _carModificationRepository = repository;
            _logger = logger;
            _carModificationMapper = carModificationMappers;
        }
        public Task<Result<CarModificationReadDto>> AddAsync(CarModificationCreateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<CarModificationReadDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<CarModificationReadDto>>> GetAllModificationsByModelId(int modelId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CarModificationReadDto>> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CarModificationReadDto>> UpdateAsync(CarModificationUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
