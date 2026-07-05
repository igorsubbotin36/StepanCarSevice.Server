using Microsoft.EntityFrameworkCore;
using StepanCarSevice.VisitService.Domain.Repositories;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;

namespace StepanCarSevice.VisitService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly VisitDBContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public Repository(VisitDBContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
        }
    }
}
