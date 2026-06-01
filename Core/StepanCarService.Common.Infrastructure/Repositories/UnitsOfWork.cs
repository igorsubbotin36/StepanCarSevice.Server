using StepanCarService.Common.Core.Repositories;
using StepanCarService.Common.Infastructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Infastructure.Repositories
{
    public class UnitsOfWork<T> : IUnitOfWork where T : TenantBaseDbContext
    {
        private readonly T _dbContext;
        public UnitsOfWork(T dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
