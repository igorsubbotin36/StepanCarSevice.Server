using StepanCarService.Common.Application.Models;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces
{
    public interface IDetailService : ITService<DetailCreateDto, DetailReadDto>
    {
        Task<Result> EditDetailAsync(DetailUpdateDto detail);
        Task<Result<List<DetailReadDto>>> GetAllDetailsAsync();
        Task<Result<List<DetailReadDto>>> GetDetailsByCodeAsync(string code);
        Task<Result<DetailReadDto>> GetDetailByIdAsync(int id);
    }
}
