using StepanCarSevice.AuthService.Domain.Entities;

namespace StepanCarService.TestKit.Builders;

// Id ролей из сида Auth (DbInitializer)
public static class RoleIds
{
    public const int GodMode = 1;
    public const int User = 2;
    public const int TenantOwner = 3;
    public const int TenantModerator = 4;
}

// Пользователь Auth. По умолчанию — пользователь портала (TenantId = null) с ролью User.
// Пароль хранится хэшем: передайте хэш от настоящего PasswordHasher (WithPasswordHash)
public sealed class UserBuilder
{
    private string? _tenantId;
    private int _roleId = RoleIds.User;
    private string _phone = TestData.Phone();
    private string _email = TestData.Email();
    private string? _firstName = TestData.FirstName();
    private string? _secondName = TestData.LastName();
    private string _passwordHash = string.Empty;
    private string _securityStamp = Guid.NewGuid().ToString("N");

    public static UserBuilder User() => new();

    public UserBuilder InTenant(string? tenantId) { _tenantId = tenantId; return this; }
    public UserBuilder WithRole(int roleId) { _roleId = roleId; return this; }
    public UserBuilder WithPhone(string phone) { _phone = phone; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithName(string? firstName, string? secondName) { _firstName = firstName; _secondName = secondName; return this; }
    public UserBuilder WithPasswordHash(string passwordHash) { _passwordHash = passwordHash; return this; }
    public UserBuilder WithSecurityStamp(string securityStamp) { _securityStamp = securityStamp; return this; }

    public User Build() => new()
    {
        Name = _phone,
        TenantId = _tenantId!,
        RoleId = _roleId,
        Phone = _phone,
        Email = _email,
        FirstName = _firstName,
        SecondName = _secondName,
        Password = _passwordHash,
        SecurityStamp = _securityStamp
    };

    public static implicit operator User(UserBuilder builder) => builder.Build();
}
