using StepanCarService.Core.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result> UpdateUserAsync(EditUserRequestDto request, string token);
        Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, string token);
        Task<Result<UserInfoDto>> GetUserInfoByPhoneAsync(string phone);
        Task<Result<List<UserInfoDto>>> GetAllUsersAsync();
    }
}
