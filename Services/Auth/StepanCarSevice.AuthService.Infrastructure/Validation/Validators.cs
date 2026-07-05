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
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password);
        }
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
            RuleFor(x => x.OldPhone).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(x => x.FirstName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FirstName));
            RuleFor(x => x.SecondName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.SecondName));
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
        }
    }

}
