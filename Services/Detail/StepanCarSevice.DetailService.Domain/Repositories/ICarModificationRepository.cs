using StepanCarSevice.DetailService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.DetailService.Domain.Repositories
{
    public interface ICarModificationRepository : IRepository<CarModification>
    {
        Task<List<CarModification>> GetAllModificationsByModelId(int modelId, string tenantId);
    }
}
