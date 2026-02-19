using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class DetailRepository : Repository<Detail>, IDetailRepository
    {
        public DetailRepository(DetailDbContext context, ILogger<DetailRepository> logger) : base(context, logger) { }

        public Task<bool> DecrementDetailsAsync(int[] id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditDetailAsync(Detail detail)
        {
            throw new NotImplementedException();
        }

        public Task<List<Detail>> GetAllDetailsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Detail>> GetDetailByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<Detail?> GetDetailByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
