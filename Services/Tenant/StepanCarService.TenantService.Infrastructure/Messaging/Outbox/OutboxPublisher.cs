using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Infrastructure.DbContexts;

namespace StepanCarService.TenantService.Infrastructure.Messaging.Outbox
{
    // Публикует события из таблицы OutboxMessages в RabbitMQ.
    //  - Строго по порядку: если событие не отправилось, более поздние ждут (удаление не обгонит создание).
    //  - Повторы с растущей паузой до 60 с, пока брокер не станет доступен.
    //  - Advisory lock в PostgreSQL: при нескольких экземплярах сервиса публикует только один.
    //  - Гарантия «хотя бы один раз»: при сбое после публикации событие уйдёт повторно, обработчики идемпотентны.
    public class OutboxPublisher : BackgroundService
    {
        // Произвольный ключ advisory lock публикатора outbox Tenant-сервиса
        private const long PublisherLockKey = 7_311_424_001;
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessageBus _messageBus;
        private readonly OutboxSettings _settings;
        private readonly ILogger<OutboxPublisher> _logger;

        public OutboxPublisher(IServiceScopeFactory scopeFactory,
            IMessageBus messageBus,
            IOptions<OutboxSettings> settings,
            ILogger<OutboxPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _messageBus = messageBus;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nextCleanupAt = DateTimeOffset.UtcNow;
            while (!stoppingToken.IsCancellationRequested)
            {
                var published = 0;
                try
                {
                    published = await PublishPendingAsync(stoppingToken);
                    if (DateTimeOffset.UtcNow >= nextCleanupAt)
                    {
                        await CleanupAsync(stoppingToken);
                        nextCleanupAt = DateTimeOffset.UtcNow + CleanupInterval;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception e)
                {
                    // Например, недоступна БД: попробуем на следующем цикле
                    _logger.LogError($"Outbox: ошибка обработки очереди событий\n{e}");
                }

                // Если что-то отправили, сразу берём следующую пачку
                if (published == 0)
                {
                    try { await Task.Delay(_settings.PollIntervalMilliseconds, stoppingToken); }
                    catch (OperationCanceledException) { return; }
                }
            }
        }

        private async Task<int> PublishPendingAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TenantServiceDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var lockTaken = await db.Database
                .SqlQueryRaw<bool>("SELECT pg_try_advisory_xact_lock({0}) AS \"Value\"", PublisherLockKey)
                .SingleAsync(cancellationToken);
            if (!lockTaken)
                return 0;

            var batch = await db.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.Id)
                .Take(_settings.BatchSize)
                .ToListAsync(cancellationToken);

            var published = 0;
            foreach (var message in batch)
            {
                if (message.NextAttemptAt > DateTimeOffset.UtcNow)
                    break; // самое раннее событие ещё ждёт повтора — более поздние не обгоняют его

                try
                {
                    await _messageBus.PublishAsync(message.Payload, message.MessageId.ToString(), cancellationToken);
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.LastError = null;
                    published++;
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    message.Attempts++;
                    message.LastError = e.Message.Length > 2000 ? e.Message[..2000] : e.Message;
                    var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, message.Attempts), MaxRetryDelay.TotalSeconds));
                    message.NextAttemptAt = DateTimeOffset.UtcNow + delay;
                    _logger.LogWarning($"Outbox: событие {message.Id} ({message.EventType}) не отправлено, попытка {message.Attempts}: {e.Message}. Повтор через {delay.TotalSeconds:0} с");
                    break;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            if (published > 0)
                _logger.LogInformation($"Outbox: отправлено событий: {published}");
            return published;
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TenantServiceDbContext>();
            var threshold = DateTimeOffset.UtcNow.AddDays(-_settings.RetentionDays);
            var removed = await db.OutboxMessages
                .Where(m => m.ProcessedAt != null && m.ProcessedAt < threshold)
                .ExecuteDeleteAsync(cancellationToken);
            if (removed > 0)
                _logger.LogInformation($"Outbox: удалено старых отправленных событий: {removed}");
        }
    }
}
