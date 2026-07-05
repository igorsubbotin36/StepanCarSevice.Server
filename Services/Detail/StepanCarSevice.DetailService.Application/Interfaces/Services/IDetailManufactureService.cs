using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface IDetailManufactureService
    {
        Task<Result<DetailManufactureReadDto>> AddAsync(DetailManufactureCreateDto model);
        Task<Result<DetailManufactureReadDto>> UpdateAsync(DetailManufactureUpdateDto model);
        Task<Result<DetailManufactureReadDto>> GetByNameOrCreateAsync(DetailManufactureCreateDto model);
        Task<Result<List<DetailManufactureReadDto>>> GetAllAsync();
        Task<Result<DetailManufactureReadDto>> GetByNameAsync(string name);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
