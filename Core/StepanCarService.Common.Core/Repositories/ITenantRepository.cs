using StepanCarService.Common.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Core.Repositories
{
    public interface ITenantRepository
    {
        Task AddAsync(TenantInfoEntity tenant);
        Task UpdateAsync(TenantInfoEntity tenant);
        Task DeleteAsync(TenantInfoEntity tenant);
        Task<TenantInfoEntity?> GetByIdAsync(string tenantId);
        Task<TenantInfoEntity?> GetByNameAsync(string tenantName);
        Task<IEnumerable<TenantInfoEntity>?> GetAllAsync();
    }
}
