using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IRepository<T> where T : AppBaseTenantsEntity
    {
        Task<T> AddAsync(T entity);
        T Update(T entity);
        void Delete(T entity);
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetByNameAsync(string name);
    }
}
