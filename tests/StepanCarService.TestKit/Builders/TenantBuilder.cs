using StepanCarService.Common.Core.Entities;

namespace StepanCarService.TestKit.Builders;

// Тенант (копия в БД сервиса или тенант Tenant-сервиса). По умолчанию активный, создан GodMode (без владельца)
public sealed class TenantBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _identifier = TestData.TenantIdentifier();
    private string? _name = TestData.CompanyName();
    private bool _isActive = true;
    private int? _ownerUserId;

    public static TenantBuilder Tenant() => new();

    public TenantBuilder WithId(string id) { _id = id; return this; }
    public TenantBuilder WithIdentifier(string identifier) { _identifier = identifier; return this; }
    public TenantBuilder WithName(string? name) { _name = name; return this; }
    public TenantBuilder Inactive() { _isActive = false; return this; }
    public TenantBuilder OwnedBy(int ownerUserId) { _ownerUserId = ownerUserId; return this; }

    public TenantInfoEntity Build() => new()
    {
        Id = _id,
        Identifier = _identifier,
        Name = _name,
        IsActive = _isActive,
        OwnerUserId = _ownerUserId
    };

    public static implicit operator TenantInfoEntity(TenantBuilder builder) => builder.Build();
}
