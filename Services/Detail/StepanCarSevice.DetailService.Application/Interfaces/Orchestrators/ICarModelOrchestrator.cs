using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface ICarModelOrchestrator
    {
        Task<Result<CarModelReadDto>> UpdateAsync(CarModelUpdateDto model);
        Task<Result<CarModelReadDto>> AddAsync(CarModelCreateDto model);
    }
}
