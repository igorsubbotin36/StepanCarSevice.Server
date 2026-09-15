using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Events;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;

namespace StepanCarService.Common.Infastructure.Messaging.Handlers
{
    // Обработчик идемпотентен: RabbitMQ может доставить одно событие повторно.
    // Failure — сообщение некорректно и повтор не поможет; исключение — ошибка обработки (например, БД), событие нужно повторить.
    public class TenantEventHandler<T> : ITenantEventHandler where T : ITenantRepository
    {
        private readonly T _tenantRepository;
        private readonly ILogger<TenantEventHandler<T>> _logger;
        public TenantEventHandler(T tenantRepository,
            ILogger<TenantEventHandler<T>> logger)
        {
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(TenantEvent tenantEvent)
        {
            if (tenantEvent == null || string.IsNullOrWhiteSpace(tenantEvent.Id))
            {
                _logger.LogError("Пришло пустое событие или событие без Id тенанта");
                return Result.Failure(ModelErrors.RequestedModelIsNull);
            }

            switch (tenantEvent.EventType)
            {
                // Registered и Updated обрабатываются одинаково (upsert):
                // повтор Registered не падает на дубликате, а Updated без Registered создаёт тенант
                case TenantEventType.Registered:
                case TenantEventType.Updated:
                    {
                        if (string.IsNullOrWhiteSpace(tenantEvent.Identifier))
                        {
                            _logger.LogError($"{tenantEvent.EventType} {tenantEvent.Id}: событие без Identifier");
                            return Result.Failure(TenantErrors.InvalidIdentifier);
                        }

                        var tenant = await _tenantRepository.GetByIdAsync(tenantEvent.Id);
                        if (tenant == null)
                        {
                            tenant = new TenantInfoEntity { Id = tenantEvent.Id };
                            Apply(tenant, tenantEvent);
                            await _tenantRepository.AddAsync(tenant);
                            _logger.LogInformation($"{tenant.Id} добавлен в сервис ({tenantEvent.EventType})");
                        }
                        else
                        {
                            Apply(tenant, tenantEvent);
                            await _tenantRepository.UpdateAsync(tenant);
                            _logger.LogInformation($"{tenant.Id} обновлен в сервисе ({tenantEvent.EventType})");
                        }
                        return Result.Success();
                    }
                case TenantEventType.Deleted:
                    {
                        var tenant = await _tenantRepository.GetByIdAsync(tenantEvent.Id);
                        if (tenant == null)
                        {
                            _logger.LogInformation($"{tenantEvent.Id} уже отсутствует в сервисе, удаление пропущено");
                            return Result.Success();
                        }
                        // Данные тенанта в сервисе удаляются вместе с ним каскадом в БД
                        await _tenantRepository.DeleteAsync(tenant);
                        _logger.LogInformation($"{tenant.Id} удален в сервисе вместе с данными");
                        return Result.Success();
                    }
                default:
                    {
                        _logger.LogError($"Не был понят тип события {tenantEvent.EventType}");
                        return Result.Failure(SystemErrors.InternalError);
                    }
            }
        }

        private static void Apply(TenantInfoEntity tenant, TenantEvent tenantEvent)
        {
            tenant.Identifier = tenantEvent.Identifier;
            tenant.Name = tenantEvent.Name;
            tenant.IsActive = tenantEvent.IsActive;
            tenant.OwnerUserId = tenantEvent.OwnerUserId;
        }
    }
}
