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
    public interface ICarManufactureService
    {
        Task<Result<CarManufactureReadDto>> AddAsync(CarManufactureCreateDto model);
        Task<Result<CarManufactureReadDto>> UpdateAsync(CarManufactureUpdateDto model);
        Task<Result<CarManufactureReadDto>> GetByNameOrCreateAsync(CarManufactureCreateDto model);
        Task<Result<List<CarManufactureReadDto>>> GetAllAsync();
        Task<Result<CarManufactureReadDto>> GetByNameAsync(string name);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
