using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : AppBaseEntity
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
            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<List<T>> GetAllAsync(string tenantId)
        {
            return await _dbSet
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, string tenantId)
        {
            return await _dbSet.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id);
        }

        public async Task<List<T>> GetByNameAsync(string name, string tenantId)
        {
            return await _dbSet.Where(x => x.TenantId == tenantId && x.Name.Contains(name))
                .ToListAsync();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
