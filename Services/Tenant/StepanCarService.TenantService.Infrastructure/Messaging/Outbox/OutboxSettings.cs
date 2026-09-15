namespace StepanCarService.TenantService.Infrastructure.Messaging.Outbox
{
    public class OutboxSettings
    {
        // Как часто проверять неотправленные события, когда очередь пуста
        public int PollIntervalMilliseconds { get; set; } = 1000;
        public int BatchSize { get; set; } = 50;
        // Сколько хранить отправленные события
        public int RetentionDays { get; set; } = 7;
    }
}
