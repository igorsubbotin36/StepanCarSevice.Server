using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;

namespace StepanCarSevice.DetailService.Application.Interfaces.Services
{
    public interface IDetailService
    {
        Task<Result<DetailReadDto>> AddAsync(DetailCreateDto model);
        Task<Result<DetailReadDto>> UpdateAsync(DetailUpdateDto model);
        Task<Result<List<DetailReadDto>>> GetAllAsync();
        Task<Result<List<DetailReadDto>>> GetByCodeAsync(string code);
        Task<Result<DetailReadDto>> GetByIdAsync(int id);
        Task<Result> DeleteByIdAsync(int Id);
    }
}
