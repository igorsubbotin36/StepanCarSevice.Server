namespace StepanCarSevice.AuthService.Application.Models.Dto
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class LoginRequestDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class EditUserRequestDto
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string OldPhone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
    public class ChangePasswordRequestDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
