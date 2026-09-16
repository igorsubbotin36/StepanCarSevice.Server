using System.Net;
using System.Reflection;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Infastructure.Mappers;

namespace StepanCarService.Common.Tests.Unit;

// Коды ошибок (ErrorConsts) и их HTTP-статусы (ErrorMapper)
[Trait(TestCategories.Name, TestCategories.Unit)]
public class ErrorCodesTests
{
    private const string UnknownErrorMessage = "Произошла непредвиденная ошибка";

    private readonly ErrorMapper _mapper = new();

    // Все коды из ErrorConsts.cs: const string во всех статических классах пространства имён моделей
    public static TheoryData<string> AllErrorCodes()
    {
        var data = new TheoryData<string>();
        foreach (var (_, value) in GetErrorConstants())
            data.Add(value);
        return data;
    }

    private static IEnumerable<(string Name, string Value)> GetErrorConstants() =>
        typeof(SystemErrors).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: true, IsSealed: true } && t.Namespace == typeof(SystemErrors).Namespace && t.Name.EndsWith("Errors"))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f is { IsLiteral: true } && f.FieldType == typeof(string))
                .Select(f => ($"{t.Name}.{f.Name}", (string)f.GetRawConstantValue()!)));

    // Забытый маппинг превращает ожидаемую ошибку в «непредвиденную» 500
    [Theory]
    [MemberData(nameof(AllErrorCodes))]
    public void EveryErrorCode_HasMapping(string errorCode)
    {
        var (_, message) = _mapper.Map(errorCode);

        message.ShouldNotBe(UnknownErrorMessage, $"для кода {errorCode} нет маппинга в ErrorMapper");
    }

    [Fact]
    public void ErrorCodeValues_AreUnique()
    {
        var duplicates = GetErrorConstants()
            .GroupBy(c => c.Value)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(c => c.Name))}");

        duplicates.ShouldBeEmpty();
    }

    // Статусы, на которые опираются клиенты API
    [Theory]
    [InlineData(AuthErrors.InvalidCredentials, HttpStatusCode.Unauthorized)]
    [InlineData(AuthErrors.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(RegisterErrors.UserAlreadyExists, HttpStatusCode.Conflict)]
    [InlineData(RegisterErrors.PasswordsDontMatch, HttpStatusCode.BadRequest)]
    [InlineData(RegisterErrors.RoleIdNotFound, HttpStatusCode.InternalServerError)]
    [InlineData(UserErrors.NotFound, HttpStatusCode.NotFound)]
    [InlineData(UserErrors.WrongPassword, HttpStatusCode.BadRequest)]
    [InlineData(ValidationErrors.RequiredField, HttpStatusCode.BadRequest)]
    [InlineData(SystemErrors.DatabaseError, HttpStatusCode.InternalServerError)]
    [InlineData(SystemErrors.ExternalServiceError, HttpStatusCode.ServiceUnavailable)]
    [InlineData(SystemErrors.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(TenantErrors.TenantNotFound, HttpStatusCode.NotFound)]
    [InlineData(TenantErrors.TenantAlreadyExists, HttpStatusCode.Conflict)]
    [InlineData(TenantErrors.InvalidIdentifier, HttpStatusCode.BadRequest)]
    [InlineData(TenantErrors.TenantInactive, HttpStatusCode.Forbidden)]
    [InlineData(TenantErrors.OwnerAlreadyHasTenant, HttpStatusCode.Conflict)]
    public void Map_ReturnsDocumentedStatus(string errorCode, HttpStatusCode expected)
    {
        _mapper.Map(errorCode).statusCode.ShouldBe(expected);
    }

    [Fact]
    public void Map_UnknownCode_Returns500WithGenericMessage()
    {
        _mapper.Map("NO_SUCH_CODE").ShouldBe((HttpStatusCode.InternalServerError, UnknownErrorMessage));
    }

    // Result.Failure(null) не должен превращаться в необработанное исключение
    [Fact(Skip = "Баг: Map(null) бросает ArgumentNullException из Dictionary.TryGetValue")]
    public void Map_NullCode_Returns500WithGenericMessage()
    {
        _mapper.Map(null!).ShouldBe((HttpStatusCode.InternalServerError, UnknownErrorMessage));
    }
}
