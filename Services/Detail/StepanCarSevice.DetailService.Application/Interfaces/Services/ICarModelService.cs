using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface ICarModelService : IService<CarModelReadDto, CarModelCreateDto, CarModelUpdateDto>
    {
        Task<Result<CarModelReadDto>> GetByNameOrCreateAsync(CarModelCreateDto model, int manufactureId);
        Task<Result<List<CarModelReadDto>>> GetByNameAsync(string name);
    }
}
