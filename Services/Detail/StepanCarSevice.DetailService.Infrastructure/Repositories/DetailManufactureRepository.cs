using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class DetailManufactureRepository : Repository<DetailManufacture>, IDetailManufactureRepository
    {
        public DetailManufactureRepository(DetailDbContext context) : base(context) { }
        public async Task<List<DetailManufacture>?> GetAllAsync(string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<DetailManufacture?> GetByIdAsync(int id, string tenantId)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.TenantId == tenantId &&  x.Id == id);
        }

        public async Task<List<DetailManufacture>?> GetByNameAsync(string name, string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId && x.Name.Contains(name))
                .ToListAsync();
        }
    }
}
