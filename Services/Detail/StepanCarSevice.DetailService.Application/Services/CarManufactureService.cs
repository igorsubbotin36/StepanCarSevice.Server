using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Services
{
    public class CarManufactureService : ICarManufactureService
    {
        private readonly ICarManufactureRepository _carManufactureRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        private readonly ILogger<CarManufactureService> _logger;
        public CarManufactureService(ICarManufactureRepository carManufactureRepository,
            IUnitOfWork unitOfWork,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ILogger<CarManufactureService> logger)
        {
            _carManufactureRepository = carManufactureRepository;
            _unitOfWork = unitOfWork;
            _accessor = accessor;
            _logger = logger;
        }

        public Task<Result<CarManufactureReadDto>> AddAsync(CarManufactureCreateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteByIdAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<CarManufactureReadDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<CarManufactureReadDto>> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CarManufactureReadDto>> GetByNameOrCreateAsync(CarManufactureCreateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CarManufactureReadDto>> UpdateAsync(CarManufactureUpdateDto model)
        {
            throw new NotImplementedException();
        }
    }
}
