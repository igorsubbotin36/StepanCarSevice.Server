using StepanCarSevice.VisitService.Domain.Entities;

namespace StepanCarSevice.VisitService.Domain.Repositories
{
    public interface ICarRepository : IRepository<CarSnapshot>
    {
        Task<bool> UpdateCarAsync(CarSnapshot car);
        Task<CarSnapshot?> GetCarByVinAsync(string win);
        Task<List<CarSnapshot>> GetCarsByUserAsync(string phone);
    }
}
