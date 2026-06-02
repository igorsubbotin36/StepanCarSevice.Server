using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void AddAsync(T entity)
        {
            _dbSet.Add(entity);
        }

        public void DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
