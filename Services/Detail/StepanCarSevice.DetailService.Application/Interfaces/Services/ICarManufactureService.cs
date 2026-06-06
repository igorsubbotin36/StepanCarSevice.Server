using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface ICarManufactureService : IService<CarManufactureReadDto, CarManufactureCreateDto, CarManufactureUpdateDto>
    {
        Task<Result<CarManufactureReadDto>> GetByNameOrCreateAsync(CarManufactureCreateDto model);
        Task<Result<List<CarManufactureReadDto>>> GetByNameAsync(string name);
    }
}
