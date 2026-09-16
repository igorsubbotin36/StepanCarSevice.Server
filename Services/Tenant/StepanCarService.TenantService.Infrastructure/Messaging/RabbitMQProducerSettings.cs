
namespace StepanCarService.TenantService.Infrastructure.Messaging
{
    public class RabbitMQProducerSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "default";
        // Имя соединения в RabbitMQ Management (диагностика; в тестах — отличить соединения прогонов)
        public string ClientProvidedName { get; set; } = "tenant-service-outbox";
    }
}
