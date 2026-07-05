using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Orchestrators
{
    public interface IDetailManufactureOrchestrator
    {
        Task<Result<DetailManufactureReadDto>> UpdateAsync(DetailManufactureUpdateDto model);
        Task<Result<DetailManufactureReadDto>> AddAsync(DetailManufactureCreateDto model);
    }
}
