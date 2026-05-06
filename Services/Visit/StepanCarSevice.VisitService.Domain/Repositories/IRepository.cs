namespace StepanCarSevice.VisitService.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<bool> AddAsync(T entity);
    }
}
