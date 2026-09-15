using StepanCarService.Common.Core.Entities;

namespace StepanCarService.TenantService.Application.Models.DTOs;

// Id и ApiKey генерируются сервером, ConnectionString клиентом не задаётся
public record TenantCreateDto(string Identifier, string Name);

// Identifier не меняется: от него зависят поддомен и данные в других сервисах.
// IsActive (подписка) меняет только GodMode; null — не менять
public record TenantUpdateDto(string Id, string Name, bool? IsActive);

// Наружу не отдаются ConnectionString и ApiKey
public record TenantReadDto(string Id, string Identifier, string Name, bool IsActive, int? OwnerUserId)
{
    public static TenantReadDto FromTenant(TenantInfoEntity t) => new TenantReadDto(t.Id,
        t.Identifier,
        t.Name,
        t.IsActive,
        t.OwnerUserId
        );
}

// Кто вызывает операцию: пользователь портала из токена
public record TenantCaller(int? UserId, bool IsGodMode);
