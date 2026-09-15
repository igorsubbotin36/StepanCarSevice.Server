using StepanCarService.Common.Application.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result> UpdateUserAsync(EditUserRequestDto request, int userId);
        Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, int userId);
        Task<Result<UserInfoDto>> GetUserInfoByPhoneAsync(string phone);
        Task<Result<List<UserInfoDto>>> GetAllUsersAsync();
    }
}
