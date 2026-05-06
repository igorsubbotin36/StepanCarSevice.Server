namespace StepanCarSevice.AuthService.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMQProducerSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "default";
        public string QueueName { get; set; } = "user.registered";
    }
}
