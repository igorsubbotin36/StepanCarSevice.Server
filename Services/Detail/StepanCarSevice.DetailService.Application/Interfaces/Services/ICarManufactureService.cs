using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface ICarManufactureService : IService<CarManufactureReadDto, CarManufactureCreateDto, CarManufactureUpdateDto>
    {
        Task<Result<CarManufactureReadDto>> GetByNameOrCreateAsync(CarManufactureCreateDto model);
        Task<Result<List<CarManufactureReadDto>>> GetByNameAsync(string name);
    }
}
