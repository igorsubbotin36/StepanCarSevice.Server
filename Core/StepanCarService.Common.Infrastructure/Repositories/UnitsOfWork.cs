using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using StepanCarService.Common.Core.Exceptions;
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
            if (_transaction == null)
                return;
            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollBackTransactionAsync()
        {
            // Транзакции может уже не быть (например, Commit упал и она освобождена): откатывать нечего
            if (_transaction == null)
                return;
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new UniqueConstraintViolationException(e);
            }
        }
    }
}
