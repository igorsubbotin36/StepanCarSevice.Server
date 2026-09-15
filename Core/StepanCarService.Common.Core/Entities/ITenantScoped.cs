namespace StepanCarService.Common.Core.Entities
{
    // Данные, принадлежащие одному тенанту. В сервисах на TenantScopedDbContext такие сущности
    // автоматически фильтруются по тенанту запроса и не могут быть записаны в чужой тенант
    public interface ITenantScoped
    {
        string TenantId { get; set; }
    }
}
