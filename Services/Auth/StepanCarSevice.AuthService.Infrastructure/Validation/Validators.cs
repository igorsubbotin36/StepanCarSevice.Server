using FluentValidation;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Infrastructure.Validation
{
    public class RegisterDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.SecondName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(PasswordRules.MinLength);
            RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password);
        }
    }

    internal static class PasswordRules
    {
        public const int MinLength = 8;
    }

    public class LoginDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Phone).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class EditUserDtoValidator : AbstractValidator<EditUserRequestDto>
    {
        public EditUserDtoValidator()
        {
            // Поле можно не передавать (null), но переданное значение не может быть пустым
            RuleFor(x => x.Email).NotEmpty().EmailAddress().When(x => x.Email != null);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30).When(x => x.Phone != null);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.FirstName != null);
            RuleFor(x => x.SecondName).NotEmpty().MaximumLength(100).When(x => x.SecondName != null);
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(PasswordRules.MinLength)
                .NotEqual(x => x.OldPassword).WithMessage("Новый пароль должен отличаться от текущего");
            RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.NewPassword);
        }
    }

}
