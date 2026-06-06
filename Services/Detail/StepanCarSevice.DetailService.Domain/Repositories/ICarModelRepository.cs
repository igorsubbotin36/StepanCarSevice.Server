using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarModelRepository : IRepository<CarModel>
    {
        Task<List<CarModel>?> GetByYearAsync(int manufactureId, int year, string tenantId);
    }
}
