using FluentValidation.TestHelper;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Infrastructure.Validation;

namespace StepanCarService.AuthService.Tests.Unit;

// Валидация тел запросов Auth (FluentValidation): границы длин и обязательность полей
[Trait(TestCategories.Name, TestCategories.Unit)]
public class ValidatorsTests
{
    private static RegisterRequestDto ValidRegister() =>
        new("owner@example.com", "Иван", "Петров", "+79990000000", "Password-1", "Password-1");

    private static string Text(int length) => new('а', length);

    [Fact]
    public void Register_ValidRequest_HasNoErrors()
    {
        new RegisterDtoValidator().TestValidate(ValidRegister()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Email", "")]
    [InlineData("Email", "not-an-email")]
    [InlineData("FirstName", "")]
    [InlineData("FirstName", "101")]
    [InlineData("SecondName", "")]
    [InlineData("SecondName", "101")]
    [InlineData("Phone", "")]
    [InlineData("Phone", "31")]
    [InlineData("Password", "7")]
    [InlineData("ConfirmPassword", "")]
    [InlineData("ConfirmPassword", "Other-password")]
    public void Register_InvalidField_HasError(string field, string value)
    {
        var request = ValidRegister();
        // Число вместо значения — строка такой длины
        var text = int.TryParse(value, out var length) ? Text(length) : value;
        request = field switch
        {
            "Email" => request with { Email = text },
            "FirstName" => request with { FirstName = text },
            "SecondName" => request with { SecondName = text },
            "Phone" => request with { Phone = text },
            "Password" => request with { Password = text, ConfirmPassword = text },
            "ConfirmPassword" => request with { ConfirmPassword = text },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        new RegisterDtoValidator().TestValidate(request).ShouldHaveValidationErrorFor(field);
    }

    // Верхние и нижние границы допустимы
    [Fact]
    public void Register_BoundaryLengths_AreValid()
    {
        var request = ValidRegister() with
        {
            FirstName = Text(100),
            SecondName = Text(100),
            Phone = Text(30),
            Password = "12345678",
            ConfirmPassword = "12345678"
        };

        new RegisterDtoValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Password-1", "Phone")]
    [InlineData("+79990000000", "", "Password")]
    public void Login_EmptyField_HasError(string phone, string password, string field)
    {
        new LoginDtoValidator().TestValidate(new LoginRequestDto(phone, password)).ShouldHaveValidationErrorFor(field);
    }

    // Непереданные поля не меняются, поэтому null допустим
    [Fact]
    public void EditUser_AllFieldsNull_IsValid()
    {
        new EditUserDtoValidator().TestValidate(new EditUserRequestDto(null, null, null, null)).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("FirstName", "")]
    [InlineData("FirstName", "101")]
    [InlineData("SecondName", " ")]
    [InlineData("SecondName", "101")]
    [InlineData("Email", "")]
    [InlineData("Email", "not-an-email")]
    [InlineData("Phone", "")]
    [InlineData("Phone", "31")]
    public void EditUser_InvalidProvidedField_HasError(string field, string value)
    {
        var text = int.TryParse(value, out var length) ? Text(length) : value;
        var request = field switch
        {
            "FirstName" => new EditUserRequestDto(text, null, null, null),
            "SecondName" => new EditUserRequestDto(null, text, null, null),
            "Email" => new EditUserRequestDto(null, null, text, null),
            "Phone" => new EditUserRequestDto(null, null, null, text),
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        new EditUserDtoValidator().TestValidate(request).ShouldHaveValidationErrorFor(field);
    }

    [Fact]
    public void EditUser_BoundaryLengths_AreValid()
    {
        new EditUserDtoValidator().TestValidate(new EditUserRequestDto(Text(100), Text(100), "user@example.com", Text(30)))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Old-password", "Old-password", "Old-password", "NewPassword")]
    [InlineData("Old-password", "1234567", "1234567", "NewPassword")]
    [InlineData("Old-password", "New-password", "", "ConfirmPassword")]
    [InlineData("Old-password", "New-password", "Other-password", "ConfirmPassword")]
    [InlineData("", "New-password", "New-password", "OldPassword")]
    public void ChangePassword_InvalidRequest_HasError(string oldPassword, string newPassword, string confirmPassword, string field)
    {
        new ChangePasswordDtoValidator().TestValidate(new ChangePasswordRequestDto(oldPassword, newPassword, confirmPassword))
            .ShouldHaveValidationErrorFor(field);
    }

    [Fact]
    public void ChangePassword_ValidRequest_HasNoErrors()
    {
        new ChangePasswordDtoValidator().TestValidate(new ChangePasswordRequestDto("Old-password", "12345678", "12345678"))
            .ShouldNotHaveAnyValidationErrors();
    }
}
