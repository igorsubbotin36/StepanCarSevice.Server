using StepanCarService.Common.Application.Models;
using StepanCarSevice.VisitService.Domain.Entities;

namespace StepanCarSevice.VisitService.Application.Interfaces
{
    public interface ICarService
    {
        Task<Result<CarSnapshot>> GetCarByVinAsync(string win);
        Task<Result<List<CarSnapshot>>> GetCarsByUserAsync(string phone);
        Task<Result> UpdateCarAsync(CarSnapshot car);
    }
}
