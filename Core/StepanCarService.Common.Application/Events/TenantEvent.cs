namespace StepanCarService.Common.Application.Events
{
    public enum TenantEventType
    {
        Registered,
        Updated,
        Deleted
    }

    // Контракт сообщения RabbitMQ. Намеренно не наследует сущность тенанта:
    // новое поле тенанта не должно попадать в другие сервисы без явного решения
    public class TenantEvent
    {
        public TenantEventType EventType { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public int? OwnerUserId { get; set; }
    }
    public class TenantRegisteredEvent : TenantEvent
    {
        public TenantRegisteredEvent()
        {
            EventType = TenantEventType.Registered;
        }
    }
    public class TenantUpdatedEvent : TenantEvent
    {
        public TenantUpdatedEvent()
        {
            EventType = TenantEventType.Updated;
        }
    }
    public class TenantDeletedEvent : TenantEvent
    {
        public TenantDeletedEvent()
        {
            EventType = TenantEventType.Deleted;
        }
    }
}
