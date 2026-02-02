using FluentValidation;

namespace StepanCarSevice.Application.Models.Validators
{
    public class RegisterModelValidator : AbstractValidator<StepanCarSevice.Application.Models.Dto.RegisterDto>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.SecondName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        }
    }

    public class LoginModelValidator : AbstractValidator<StepanCarSevice.Application.Models.Dto.LoginDto>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.Phone).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class EditUserModelValidator : AbstractValidator<StepanCarSevice.Application.Models.Dto.EditUserDto>
    {
        public EditUserModelValidator()
        {
            RuleFor(x => x.OldPhone).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
            RuleFor(x => x.FirstName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FirstName));
            RuleFor(x => x.SecondName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.SecondName));
        }
    }

    public class ChangePasswordModelValidator : AbstractValidator<StepanCarSevice.Application.Models.Dto.ChangePasswordDto>
    {
        public ChangePasswordModelValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
        }
    }
}


