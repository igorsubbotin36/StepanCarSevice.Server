using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarModificationRepository : IRepository<CarModification>
    {
        Task<List<CarModification>> GetAllModificationsByModelId(int modelId, string tenantId);
    }
}
