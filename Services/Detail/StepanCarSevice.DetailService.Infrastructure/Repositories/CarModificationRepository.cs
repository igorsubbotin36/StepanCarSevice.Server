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
    public class CarModificationRepository : Repository<CarModification>, ICarModificationRepository
    {
        public CarModificationRepository(DetailDbContext context) : base(context) { }
        public async Task<List<CarModification>> GetAllModificationsByModelId(int modelId, string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId && x.CarModelId == modelId)
                .ToListAsync();
        }
    }
}
