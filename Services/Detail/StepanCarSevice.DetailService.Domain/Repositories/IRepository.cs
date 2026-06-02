namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        void AddAsync(T entity);
        void UpdateAsync(T entity);
        void DeleteAsync(T entity);
    }
}
