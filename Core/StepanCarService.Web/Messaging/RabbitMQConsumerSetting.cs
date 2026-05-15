namespace StepanCarService.Web.Messaging
{
    public class RabbitMQConsumerSetting
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "default";
        public string QueueName { get; set; } = "default.queue";
    }
}
