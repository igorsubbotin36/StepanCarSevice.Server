namespace StepanCarService.Common.Infastructure.Messaging
{
    public class RabbitMQConsumerSetting
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "default";
        public string QueueName { get; set; } = "default.queue";
        // Сколько сообщений брокер отдаёт consumer'у без подтверждения
        public ushort PrefetchCount { get; set; } = 10;
        // Попыток для постоянных ошибок (например, нарушение ограничения в БД), после чего сообщение уходит в очередь «.dead».
        // Временные ошибки (БД или сеть недоступны) повторяются без ограничения, пока сервис работает
        public int MaxFailedAttempts { get; set; } = 5;
        // Пауза перед повтором обработки: начинается с базовой и удваивается до максимальной
        public int RetryBaseDelayMilliseconds { get; set; } = 1000;
        public int RetryMaxDelayMilliseconds { get; set; } = 60_000;
    }
}
