using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.VisitService.Domain.Entities;
using StepanCarSevice.VisitService.Domain.Repositories;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;

namespace StepanCarSevice.VisitService.Infrastructure.Repositories
{
    public class CarRepository : Repository<CarSnapshot>, ICarRepository
    {
        public CarRepository(VisitDBContext context, ILogger<CarRepository> logger) : base(context, logger) { }
        public async Task<CarSnapshot?> GetCarByVinAsync(string win)
        {
            return await _dbSet
                .Include(x => x.CarModelSnapshot)
                .Include(x => x.OwnerSnapshot)
                .SingleOrDefaultAsync(x => x.VIN == win);
        }

        public async Task<List<CarSnapshot>> GetCarsByUserAsync(string phone)
        {
            return await _dbSet
                .Where(x => x.OwnerSnapshot.Phone == phone)
                .Include(x => x.CarModelSnapshot)
                .Include(x => x.OwnerSnapshot)
                .ToListAsync();
        }

        public async Task<bool> UpdateCarAsync(CarSnapshot car)
        {
            try
            {
                _dbSet.Update(car);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Изменение информации у {car.VIN}");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка изменения информации у {car.VIN}", e);
                return false;
            }
        }
    }
}
