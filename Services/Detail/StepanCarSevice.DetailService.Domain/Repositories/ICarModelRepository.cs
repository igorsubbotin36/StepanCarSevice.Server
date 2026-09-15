using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarModelRepository : IRepository<CarModel>
    {
        Task<List<CarModel>?> GetByYearAsync(int manufactureId, int year);
    }
}
