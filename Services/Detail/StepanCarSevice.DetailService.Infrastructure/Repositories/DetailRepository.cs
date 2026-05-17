using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class DetailRepository : Repository<Detail>, IDetailRepository
    {
        public DetailRepository(DetailDbContext context) : base(context) { }

        public async Task DecrementDetailsAsync(int[] id)
        {
            throw new NotImplementedException();
        }

        public async Task EditDetailAsync(Detail detail)
        {
            _dbSet.Update(detail);
            await _dbContext.SaveChangesAsync();
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
