using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class DetailRepository : Repository<Detail>, IDetailRepository
    {
        public DetailRepository(DetailDbContext context, ILogger<DetailRepository> logger) : base(context, logger) { }

        public async Task<bool> DecrementDetailsAsync(int[] id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EditDetailAsync(Detail detail)
        {
            try
            {
                _dbSet.Update(detail);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Изменение информации у {detail.Name}");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка изменения информации у {detail.Name}", e);
                return false;
            }
        }

        public async Task<List<Detail>> GetAllDetailsAsync()
        {
            return await _dbSet
                .Include(x => x.CarModel)
                .ToListAsync();
        }

        public async Task<List<Detail>?> GetDetailsByCodeAsync(string code)
        {
            return await _dbSet
                .Where(x => x.Code == code)
                .Include(x => x.CarModel)
                .ToListAsync();
        }

        public async Task<Detail?> GetDetailByIdAsync(int id)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.Id == id);
        }
    }
}
