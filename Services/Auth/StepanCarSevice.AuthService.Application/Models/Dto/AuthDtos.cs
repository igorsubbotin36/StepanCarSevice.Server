namespace StepanCarSevice.AuthService.Application.Models.Dto
{
    public record RegisterRequestDto(string Email, string FirstName, string SecondName, string Phone, string Password, string ConfirmPassword);
    public record LoginRequestDto(string Phone, string Password);
    public record EditUserRequestDto(string FirstName, string SecondName, string OldPhone, string Email, string? Phone);
    public record ChangePasswordRequestDto(string OldPassword, string NewPassword, string ConfirmPassword);
    public record AuthResponseDto(string AccessToken);
}
