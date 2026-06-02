using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarManufactureRepository : IRepository<CarManufacture>
    {
        Task<List<CarManufacture>?> GetAllAsync(string tenantId);
        Task<CarManufacture?> GetByIdAsync(int id, string tenantId);
        Task<List<CarManufacture>?> GetByNameAsync(string name, string tenantId);
    }
}
