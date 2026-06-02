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
    public class CarModelRepository : Repository<CarModel>, ICarModelRepository
    {
        public CarModelRepository(DetailDbContext context) : base(context) { }

        public async Task<List<CarModel>?> GetAllAsync(string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<CarModel?> GetByIdAsync(int id, string tenantId)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id);
        }

        public async Task<List<CarModel>?> GetByNameAsync(string name, string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId && x.NameEN.Contains(name))
                .ToListAsync();
        }

        public async Task<List<CarModel>?> GetByYearAsync(int manufactureId, int year, string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId 
                    && x.ManufactureId == manufactureId
                    && x.YearFrom <= year
                    && x.YearTo >= year)
                .ToListAsync();
        }
    }
}
