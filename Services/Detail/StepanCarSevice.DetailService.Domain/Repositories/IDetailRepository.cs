using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IDetailRepository : IRepository<Detail>
    {
        Task<List<Detail>?> GetDetailsByCodeAsync(string code, string? tenantId);
        Task<List<Detail>?> GetByNameAsync(string name, string tenantId);
        Task<Detail?> GetByIdAsync(int id, string tenantId);
        Task<List<Detail>> GetAllAsync(string tenantId);
    }
}
