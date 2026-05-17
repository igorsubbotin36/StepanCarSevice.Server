namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
    }
}
