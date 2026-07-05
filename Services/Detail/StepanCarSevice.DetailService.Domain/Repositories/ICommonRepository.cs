using StepanCarService.Common.Core.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICommonRepository<T> where T : AppCommonEntity
    {
        Task<T> AddAsync(T entity);
        T Update(T entity);
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetByNameAsync(string name);
    }
}
