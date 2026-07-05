using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;

namespace StepanCarService.Common.Infastructure.Messaging.Handlers
{
    public class TenantRegisteredHandler<T> : ITenantRegisteredHandler where T : ITenantRepository
    {
        private readonly T _tenantRepository;
        private readonly ILogger<TenantRegisteredHandler<T>> _logger;
        public TenantRegisteredHandler(T tenantRepository,
            ILogger<TenantRegisteredHandler<T>> logger)
        {
            _tenantRepository = tenantRepository;
            _logger = logger;
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
                                _logger.LogInformation($"{newTenant.Id} добавлен в сервис");
                                return Result.Success();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"{newTenant.Id} ошибка добавления в сервис\n{e}");
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
                                _logger.LogError($"{tenantEvent.Id} не найден для обновления!");
                                return Result.Failure(ModelErrors.RequestedModelIsNull);
                            }
                            try
                            {
                                await _tenantRepository.UpdateAsync(tenant);
                                _logger.LogInformation($"{tenant.Id} обновлен в сервисе");
                                return Result.Success();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"{tenant.Id} ошибка обновления в сервисе\n{e}");
                                return Result.Failure(SystemErrors.DatabaseError);
                            }
                        }
                    case TenantEventType.Deleted:
                        {
                            var tenant = await _tenantRepository.GetByIdAsync(tenantEvent.Id);
                            if (tenant == null)
                            {
                                _logger.LogWarning($"{tenantEvent.Id} не найден для удаления!");
                                return Result.Failure(ModelErrors.RequestedModelIsNull);
                            }
                            try
                            {
                                await _tenantRepository.DeleteAsync(tenant);
                                _logger.LogInformation($"{tenant.Id} удален в сервисе");
                                return Result.Success();
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"{tenant.Id} ошибка удаления в сервисе\n{e}");
                                return Result.Failure(SystemErrors.DatabaseError);
                            }
                        }
                    default:
                        {
                            _logger.LogWarning($"Не был понят тип события {tenantEvent.EventType}");
                            return Result.Failure(SystemErrors.InternalError);
                        }
                }
            }
            else
            {
                _logger.LogError("Пришло пустое событие");
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            }
        }
    }
}
