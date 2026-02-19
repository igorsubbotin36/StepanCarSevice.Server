using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.DetailService.Domain.Repositories;
using StepanCarSevice.DetailService.Infrastructure.DBContexts;

namespace StepanCarSevice.DetailService.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DetailDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<Repository<T>> _logger;
        public Repository(DetailDbContext context, ILogger<Repository<T>> logger)
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
