namespace StepanCarService.TenantService.Infrastructure.Messaging.Outbox
{
    public class OutboxMessage
    {
        // Порядок публикации
        public long Id { get; set; }
        // MessageId в RabbitMQ
        public Guid MessageId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; set; }
        public DateTimeOffset NextAttemptAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }
    }
}
