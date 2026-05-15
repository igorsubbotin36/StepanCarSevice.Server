using StepanCarService.Core.Entities;
using StepanCarService.Core.Events;
using StepanCarService.Core.Interfaces.Repositories;
using StepanCarService.Core.Models;
using StepanCarSevice.AuthService.Application.Interfaces;

namespace StepanCarService.Web.Messaging.Handlers
{
    public class TenantRegisteredHandler<T> : ITenantRegisteredHandler where T : ITenantRepository
    {
        private readonly T _tenantRepository;
        public TenantRegisteredHandler(T tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Result> HandleAsync(TenantEvent tenantEvent)
        {
            if (tenantEvent != null)
            {
                switch (tenantEvent.EventType)
                {
                    case TenantEventType.Registered:
                        {
                            TenantInfoEntity newTenant = new TenantInfoEntity();
                            newTenant.Id = tenantEvent.Id;
                            newTenant.Name = tenantEvent.Name;
                            newTenant.IsActive = tenantEvent.IsActive;
                            newTenant.ApiKey = tenantEvent.ApiKey;
                            newTenant.ConnectionString = tenantEvent.ConnectionString;
                            newTenant.Identifier = tenantEvent.Identifier;
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
                    case TenantEventType.Updated:
                        {
                            var tenant = await _tenantRepository.GetByIdAsync(tenantEvent.Id);
                            if (tenant != null)
                            {
                                tenant.Id = tenantEvent.Id;
                                tenant.Name = tenantEvent.Name;
                                tenant.IsActive = tenantEvent.IsActive;
                                tenant.ApiKey = tenantEvent.ApiKey;
                                tenant.ConnectionString = tenantEvent.ConnectionString;
                                tenant.Identifier = tenantEvent.Identifier;
                            }
                            else
                            {
                                Console.WriteLine("Не найден тенант для обновления");
                                return Result.Failure(ModelErrors.RequestedModelIsNull);
                            }
                            try
                            {
                                await _tenantRepository.UpdateAsync(tenant);
                                return Result.Success();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return Result.Failure(SystemErrors.DatabaseError);
                            }
                        }
                    case TenantEventType.Deleted:
                        {
                            var tenant = await _tenantRepository.GetByIdAsync(tenantEvent.Id);
                            if (tenant == null)
                            {
                                Console.WriteLine("Не найден тенант для удаления");
                                return Result.Failure(ModelErrors.RequestedModelIsNull);
                            }
                            try
                            {
                                await _tenantRepository.DeleteAsync(tenant);
                                return Result.Success();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return Result.Failure(SystemErrors.DatabaseError);
                            }
                        }
                    default:
                        {
                            Console.WriteLine("Внутренняя ошибка");
                            return Result.Failure(SystemErrors.InternalError);
                        }
                }
            }
            else
            {
                Console.WriteLine("Пустая модель тенанта для действия");
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            }
        }
    }
}
