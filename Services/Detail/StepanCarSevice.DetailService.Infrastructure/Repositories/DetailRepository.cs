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

        public async Task DecrementDetailsAsync(int[] id, string? tenantId)
        {
            throw new NotImplementedException();
        }

        public async Task EditDetailAsync(Detail detail)
        {
            _dbSet.Update(detail);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Detail>> GetAllDetailsAsync(string? tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId)
                .Include(x => x.CarModel)
                .ToListAsync();
        }

        public async Task<List<Detail>?> GetDetailsByCodeAsync(string code, string? tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.Code == code)
                .Include(x => x.CarModel)
                .ToListAsync();
        }

        public async Task<Detail?> GetDetailByIdAsync(int id, string? tenantId)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id);
        }
    }
}
