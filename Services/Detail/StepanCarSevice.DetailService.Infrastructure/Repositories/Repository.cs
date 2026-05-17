using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DetailDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public Repository(DetailDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
