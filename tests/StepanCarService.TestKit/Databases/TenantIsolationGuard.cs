using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;

namespace StepanCarService.TestKit.Databases;

// Guard (CD-75): каждая сущность модели со свойством TenantId изолирована по тенанту —
// реализует ITenantScoped и имеет фильтр запросов. Ловит новую сущность, добавленную без изоляции
public static class TenantIsolationGuard
{
    public static IReadOnlyList<string> FindViolations(DbContext db)
    {
        var violations = new List<string>();
        foreach (var entityType in db.Model.GetEntityTypes())
        {
            if (entityType.FindProperty(nameof(ITenantScoped.TenantId)) == null)
                continue;

            var name = entityType.DisplayName();
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                violations.Add($"{name}: есть TenantId, но сущность не реализует ITenantScoped");
            else if (entityType.GetRootType().GetQueryFilter() == null)
                violations.Add($"{name}: нет фильтра запросов по тенанту");
        }
        return violations;
    }
}
