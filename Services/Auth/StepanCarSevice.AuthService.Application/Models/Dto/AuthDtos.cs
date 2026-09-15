namespace StepanCarSevice.AuthService.Application.Models.Dto
{
    public record RegisterRequestDto(string Email, string FirstName, string SecondName, string Phone, string Password, string ConfirmPassword);
    public record LoginRequestDto(string Phone, string Password);
    // TenantId намеренно отсутствует: привязка к тенанту меняется только серверной логикой
    public record EditUserRequestDto(string? FirstName, string? SecondName, string? Email, string? Phone);
    public record ChangePasswordRequestDto(string OldPassword, string NewPassword, string ConfirmPassword);
    public record AuthResponseDto(string AccessToken);
}
