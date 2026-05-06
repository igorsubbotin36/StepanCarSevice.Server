using StepanCarService.Core.Models;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Application.Interfaces
{
    public interface IDetailService
    {
        Task<Result> EditDetailAsync(Detail detail);
        Task<Result<List<Detail>>> GetAllDetailsAsync();
        Task<Result<List<Detail>>> GetDetailsByCodeAsync(string code);
        Task<Result<Detail>> GetDetailByIdAsync(int id);
    }
}
