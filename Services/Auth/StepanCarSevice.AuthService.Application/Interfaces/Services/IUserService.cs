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
        Task<bool> UpdateUserAsync(int userId, EditUserRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Result<UserDto> GetClaims(string token);
    }
}
