using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface ICarManufactureOrchestrator
    {
        Task<Result<CarManufactureReadDto>> UpdateAsync(CarManufactureUpdateDto model);
        Task<Result<CarManufactureReadDto>> AddAsync(CarManufactureCreateDto model);
    }
}
