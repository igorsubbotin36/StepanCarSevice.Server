using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface IDetailOrchestrator
    {
        Task<Result<DetailReadDto>> UpdateAsync(DetailUpdateDto model);
        Task<Result<DetailReadDto>> AddAsync(DetailCreateDto model);
    }
}
