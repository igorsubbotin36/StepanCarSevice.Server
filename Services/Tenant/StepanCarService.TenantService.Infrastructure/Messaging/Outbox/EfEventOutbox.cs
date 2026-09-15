using System.Text.Json;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Infrastructure.DbContexts;

namespace StepanCarService.TenantService.Infrastructure.Messaging.Outbox
{
    public class EfEventOutbox : IEventOutbox
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly TenantServiceDbContext _dbContext;

        public EfEventOutbox(TenantServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Enqueue<T>(T message) where T : class
        {
            var now = DateTimeOffset.UtcNow;
            _dbContext.OutboxMessages.Add(new OutboxMessage
            {
                MessageId = Guid.NewGuid(),
                EventType = message.GetType().Name,
                // Runtime-тип: наследники события сериализуются со всеми полями
                Payload = JsonSerializer.Serialize(message, message.GetType(), JsonOptions),
                OccurredAt = now,
                NextAttemptAt = now
            });
        }
    }
}
