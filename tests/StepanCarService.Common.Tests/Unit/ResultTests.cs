using StepanCarService.Common.Application.Models;

namespace StepanCarService.Common.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class ResultTests
{
    [Fact]
    public void Success_WithoutValue_IsSuccessWithoutErrorCode()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.ErrorCode.ShouldBeNull();
    }

    [Fact]
    public void Success_WithValue_KeepsValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Failure_KeepsErrorCodeAndDefaultValue()
    {
        var result = Result.Failure<string>(SystemErrors.DatabaseError);

        result.IsSuccess.ShouldBeFalse();
        result.ErrorCode.ShouldBe(SystemErrors.DatabaseError);
        result.Value.ShouldBeNull();
    }
}
