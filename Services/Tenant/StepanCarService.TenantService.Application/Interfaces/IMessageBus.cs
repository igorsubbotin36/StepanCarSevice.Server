namespace StepanCarService.TenantService.Application.Interfaces
{
    public interface IMessageBus
    {
        // Публикует уже сериализованное сообщение. Завершается только после подтверждения брокера,
        // при любой ошибке бросает исключение
        Task PublishAsync(string payload, string messageId, CancellationToken cancellationToken = default);
    }
}
