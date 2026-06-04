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
    public interface ICarModelService
    {
        Task<Result<CarModelReadDto>> AddAsync(CarModelCreateDto model, int manufactureId);
        Task<Result<CarModelReadDto>> UpdateAsync(CarModelUpdateDto model, int manufactureId);
        Task<Result<CarModelReadDto>> GetByNameOrCreateAsync(CarModelCreateDto model, int manufactureId);
        Task<Result<List<CarModelReadDto>>> GetAllAsync();
        Task<Result<CarModelReadDto>> GetByNameAsync(string name);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
