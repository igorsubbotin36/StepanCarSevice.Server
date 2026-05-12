using StepanCarService.Core.Entities;
using StepanCarService.Core.Events;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.Core.Models;
using StepanCarSevice.AuthService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Web.Handlers
{
    public class TenantRegisteredHandler<T> : ITenantRegisteredHandler where T : ITenantRepository
    {
        private readonly T _tenantRepository;
        public TenantRegisteredHandler(T tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Result> HandleAsync(TenantRegisteredEvent tenantRegisteredEvent)
        {
            TenantInfoEntity newTenant = new TenantInfoEntity();
            if (tenantRegisteredEvent != null)
            {
                newTenant.Id = tenantRegisteredEvent.Id;
                newTenant.Name = tenantRegisteredEvent.Name;
                newTenant.IsActive = tenantRegisteredEvent.IsActive;
                newTenant.ApiKey = tenantRegisteredEvent.ApiKey;
                newTenant.ConnectionString = tenantRegisteredEvent.ConnectionString;
                newTenant.Identifier = tenantRegisteredEvent.Identifier;
            }
            else
            {
                Console.WriteLine("Пустая модель тенанта для регистрации");
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            }
            try
            {
                await _tenantRepository.AddAsync(newTenant);
                return Result.Success();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }
    }
}
