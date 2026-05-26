using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IDetailRepository : IRepository<Detail>
    {
        Task<List<Detail>?> GetDetailsByCodeAsync(string code, string? tenantId);
        Task<List<Detail>> GetAllDetailsAsync(string? tenantId);
        Task<Detail?> GetDetailByIdAsync(int id, string? tenantId);
        Task EditDetailAsync(Detail detail);
        Task DecrementDetailsAsync(int[] id, string? tenantId);
    }
}
