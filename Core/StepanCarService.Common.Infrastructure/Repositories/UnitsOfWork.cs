using Microsoft.EntityFrameworkCore.Storage;
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
        private IDbContextTransaction _transaction;
        public UnitsOfWork(T dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollBackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
