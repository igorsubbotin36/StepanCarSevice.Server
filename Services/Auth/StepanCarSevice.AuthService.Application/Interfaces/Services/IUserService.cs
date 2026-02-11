using StepanCarSevice.AuthService.Application.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result> RegisterAsync(RegisterRequestDto request);
        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result> UpdateUserAsync(EditUserRequestDto request, string token);
        Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, string token);
        Result<UserDto> GetClaims();
        Task<Result<UserInfoDto>> GetUserInfoByPhoneAsync(string phone);
    }
}
