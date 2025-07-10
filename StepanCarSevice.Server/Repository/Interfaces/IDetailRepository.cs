using StepanCarSevice.Server.Entities;
using StepanCarSevice.Server.Models;

namespace StepanCarSevice.Server.Repository.Interfaces
{
    public interface IDetailRepository
    {
        public Task<List<Detail>> GetDetailByCode(string code);
        public Task<List<Detail>> GetAllDetails();
        public Task<Detail?> GetDetailById(int id);
        public Task<bool> AddDetail(Detail detail);
        public Task<bool> EditDetail(Detail detail);
        public Task<bool> DeleteDetail(int id);
    }
}
