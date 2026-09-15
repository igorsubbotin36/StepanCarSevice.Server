namespace StepanCarService.TenantService.Application.Interfaces
{
    // Transactional Outbox: событие сохраняется в БД в той же транзакции, что и изменение данных,
    // и публикуется в брокер фоновым процессом. Событие не теряется, даже если брокер недоступен
    public interface IEventOutbox
    {
        // Добавляет событие в текущий unit of work; записывается в БД при SaveChanges
        void Enqueue<T>(T message) where T : class;
    }
}
