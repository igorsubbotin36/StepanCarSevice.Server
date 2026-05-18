using StepanCarService.Core.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result> RegisterAsync(RegisterRequestDto request);
        Result<UserDto> GetClaims();
    }
}
