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

        public async Task<List<Detail>?> GetDetailsByCodeAsync(string code, string? tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId && x.Code == code)
                .Include(x => x.CarModifications)
                .ToListAsync();
        }
    }
}
