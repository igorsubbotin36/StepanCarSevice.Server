using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        public CarRepository(DetailDbContext context, ILogger<CarRepository> logger) : base(context, logger) { }
        public Task<Car> GetCarByWinAsync(string win)
        {
            throw new NotImplementedException();
        }

        public Task<List<Car>> GetCarsByUserAsync(string phone)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCarAsync(Car car)
        {
            throw new NotImplementedException();
        }
    }
}
