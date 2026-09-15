using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class CarModificationRepository : Repository<CarModification>, ICarModificationRepository
    {
        public CarModificationRepository(DetailDbContext context) : base(context) { }
        public async Task<List<CarModification>> GetAllModificationsByModelId(int modelId)
        {
            return await _dbSet.Where(x => x.CarModelId == modelId)
                .ToListAsync();
        }
    }
}
