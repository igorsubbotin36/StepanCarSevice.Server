using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.VisitService.Domain.Repositories;
using StepanCarSevice.VisitService.Infrastructure.DBContexts;

namespace StepanCarSevice.VisitService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly VisitDBContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;
        public Repository(VisitDBContext context, ILogger<Repository<T>> logger)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        public async Task<bool> AddAsync(T entity)
        {
            try
            {
                _dbSet.Add(entity);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"{entity.ToString()} добавлено!");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка при добавлении {entity.ToString()} в БД", e);
                return false;
            }
        }
    }
}
