using StepanCarService.Common.Core.Entities;

namespace StepanCarService.Common.Application.Events
{
    public enum TenantEventType
    {
        Registered,
        Updated,
        Deleted
    }
    public class TenantEvent : TenantInfoEntity
    {
        public TenantEventType EventType { get; set; }
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
