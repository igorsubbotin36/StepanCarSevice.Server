using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IDetailRepository : IRepository<Detail>
    {
        Task<List<Detail>> GetDetailByCodeAsync(string code);
        Task<List<Detail>> GetAllDetailsAsync();
        Task<Detail?> GetDetailByIdAsync(int id);
        Task<bool> EditDetailAsync(Detail detail);
        Task<bool> DecrementDetailsAsync(int[] id);
    }
}
