using StepanCarSevice.Domain.Entities;

namespace StepanCarSevice.Application.Repository.Interfaces
{
    public interface IDetailRepository
    {
        public Task<List<Detail>> GetDetailByCode(string code);
        public Task<List<Detail>> GetAllDetails();
        public Task<Detail?> GetDetailById(int id);
        public Task<bool> AddDetail(Detail detail);
        public Task<bool> EditDetail(Detail detail);
        public Task<bool> DeleteDetails(int[] id);
    }
}
