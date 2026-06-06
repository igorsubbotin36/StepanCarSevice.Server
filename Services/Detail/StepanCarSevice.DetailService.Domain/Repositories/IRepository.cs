using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IRepository<T> where T : AppBaseEntity
    {
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T> GetByIdAsync (int id, string tenantId);
        Task<List<T>> GetAllAsync(string tenantId);
        Task<List<T>> GetByNameAsync(string name, string tenantId);
    }
}
