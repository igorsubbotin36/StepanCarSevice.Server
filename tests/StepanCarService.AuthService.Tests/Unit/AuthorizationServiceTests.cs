using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.MultiTenancy;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Application.Services;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using StepanCarSevice.AuthService.Infrastructure.Auth;

namespace StepanCarService.AuthService.Tests.Unit;

// Регистрация и вход на фейках: то, что трудно или невозможно проверить через HTTP
[Trait(TestCategories.Name, TestCategories.Unit)]
public class AuthorizationServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly FakeLogger<AuthorizationService> _logger = new();

    private AuthorizationService CreateService(TenantInfoEntity? tenant = null) => new(
        _users,
        _hasher,
        Substitute.For<ITokenGeneratorService>(),
        Substitute.For<ICurrentUserService>(),
        _logger,
        new FakeTenantAccessor(tenant),
        Substitute.For<IUnitOfWork>());

    private static RegisterRequestDto Register(string phone, string password = "Password-1", string? confirm = null) =>
        new("owner@example.com", "Иван", "Петров", phone, password, confirm ?? password);

    // Проверка в сервисе (через HTTP несовпадение раньше отлавливает валидатор)
    [Fact]
    public async Task Register_PasswordsDontMatch_Fails()
    {
        var result = await CreateService().RegisterAsync(Register(TestData.Phone(), "Password-1", "Password-2"));

        result.ErrorCode.ShouldBe(RegisterErrors.PasswordsDontMatch);
        await _users.DidNotReceiveWithAnyArgs().AddUserAsync(default!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Register_RoleMissingInDatabase_FailsWithRoleIdNotFound(bool inTenant)
    {
        _users.GetRoleIdAsync(Arg.Any<string>()).Returns((int?)null);

        var result = await CreateService(inTenant ? TenantBuilder.Tenant().Build() : null).RegisterAsync(Register(TestData.Phone()));

        result.ErrorCode.ShouldBe(RegisterErrors.RoleIdNotFound);
        await _users.DidNotReceiveWithAnyArgs().AddUserAsync(default!);
    }

    // В лог попадает Id пользователя, но не телефон
    [Fact]
    public async Task Register_LogsUserIdWithoutPhone()
    {
        var phone = TestData.Phone();
        _users.GetRoleIdAsync(Roles.TenantOwner).Returns(RoleIds.TenantOwner);
        _users.When(r => r.AddUserAsync(Arg.Any<User>())).Do(call => call.Arg<User>().Id = 4242);

        var result = await CreateService().RegisterAsync(Register(phone));

        result.IsSuccess.ShouldBeTrue();
        var messages = _logger.Collector.GetSnapshot().Select(r => r.Message).ToList();
        messages.ShouldContain(m => m.Contains("4242"));
        messages.ShouldAllBe(m => !m.Contains(phone) && !m.Contains(phone.TrimStart('+')));
    }

    // Для неизвестного телефона хэш всё равно проверяется — время ответа не выдаёт, зарегистрирован ли номер
    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public async Task FindUserForLogin_UnknownPhone_StillVerifiesHash(bool inTenant, int expectedVerifications)
    {
        // Хэш-заглушка кэшируется в статическом поле сервиса на весь процесс: отдаём настоящий хэш,
        // чтобы не сломать выравнивание времени в API-тестах этого же процесса
        _hasher.Hash(Arg.Any<string>()).Returns(new PasswordHasher().Hash(Guid.NewGuid().ToString()));
        _users.GetUserByPhoneAsync(Arg.Any<string>(), Arg.Any<string?>()).Returns((User?)null);

        var user = await CreateService(inTenant ? TenantBuilder.Tenant().Build() : null).FindUserForLoginAsync(TestData.Phone(), "Password-1");

        user.ShouldBeNull();
        _hasher.Received(expectedVerifications).Verify("Password-1", Arg.Is<string>(h => !string.IsNullOrEmpty(h)));
    }

    // У пользователя тенанта и владельца один телефон — пароль определяет, кто входит
    [Theory]
    [InlineData("tenant-user-password", Roles.User)]
    [InlineData("owner-password", Roles.TenantOwner)]
    public async Task FindUserForLogin_SamePhoneInTenantAndPortal_PasswordSelectsAccount(string password, string expectedRole)
    {
        var phone = TestData.Phone();
        var owner = new User { Id = 7, Name = "owner", Password = "owner-hash", Role = new Role { Name = Roles.TenantOwner, Description = "" } };
        var tenant = TenantBuilder.Tenant().OwnedBy(owner.Id).Build();
        var tenantUser = new User { Id = 8, Name = "user", TenantId = tenant.Id!, Password = "user-hash", Role = new Role { Name = Roles.User, Description = "" } };
        _users.GetUserByPhoneAsync(phone, tenant.Id).Returns(tenantUser);
        _users.GetUserByPhoneAsync(phone, null).Returns(owner);
        _hasher.Verify("tenant-user-password", "user-hash").Returns(true);
        _hasher.Verify("owner-password", "owner-hash").Returns(true);

        var user = await CreateService(tenant).FindUserForLoginAsync(phone, password);

        user.ShouldNotBeNull().Role.Name.ShouldBe(expectedRole);
    }
}
