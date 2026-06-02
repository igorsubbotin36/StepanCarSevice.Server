using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface IDetailManufactureRepository : IRepository<DetailManufacture>
    {
        Task<List<DetailManufacture>?> GetAllAsync(string tenantId);
        Task<DetailManufacture?> GetByIdAsync(int id, string tenantId);
        Task<List<DetailManufacture>?> GetByNameAsync(string name, string tenantId);
    }
}
