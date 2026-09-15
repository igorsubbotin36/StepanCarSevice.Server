using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class CarModelRepository : Repository<CarModel>, ICarModelRepository
    {
        public CarModelRepository(DetailDbContext context) : base(context) { }

        public async Task<List<CarModel>?> GetByYearAsync(int manufactureId, int year)
        {
            return await _dbSet.Where(x => x.ManufactureId == manufactureId
                    && x.YearFrom <= year
                    && x.YearTo >= year)
                .ToListAsync();
        }
    }
}
