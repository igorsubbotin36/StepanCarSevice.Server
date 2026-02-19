using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarRepository : IRepository<Car>
    {
        Task<bool> UpdateCarAsync(Car car);
        Task<Car> GetCarByWinAsync(string win);
        Task<List<Car>> GetCarsByUserAsync(string phone);
    }
}
