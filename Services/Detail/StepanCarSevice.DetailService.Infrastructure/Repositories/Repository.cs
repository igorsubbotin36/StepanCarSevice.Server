using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : AppBaseTenantsEntity
    {
        protected readonly DetailDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        public Repository(DetailDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet
                .ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<T>> GetByNameAsync(string name)
        {
            return await _dbSet.Where(x => x.Name.Contains(name))
                .ToListAsync();
        }

        public T Update(T entity)
        {
            _dbSet.Update(entity);
            return entity;
        }
    }
}
