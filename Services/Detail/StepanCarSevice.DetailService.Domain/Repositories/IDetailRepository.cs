using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IDetailRepository : IRepository<Detail>
    {
        Task<List<Detail>> GetDetailsByCodeAsync(string code);
        Task<List<Detail>> GetAllDetailsAsync();
        Task<Detail?> GetDetailByIdAsync(int id);
        Task EditDetailAsync(Detail detail);
        Task DecrementDetailsAsync(int[] id);
    }
}
