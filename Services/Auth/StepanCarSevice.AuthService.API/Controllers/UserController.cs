using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarSevice.AuthService.Application.Extensions;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ApiControllerBase<UserController>
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService, ILogger<UserController> logger, IErrorMapper errorMapper) : base(errorMapper, logger)
        {
            _userService = userService;
        }
        [HttpGet("getUserInfo")]
        [Authorize(Roles = "GodMode")]
        public async Task<IActionResult> GetUserInfoAsync(string phone)
        {
            var result = await _userService.GetUserInfoByPhoneAsync(phone);
            return HandleResult(result);
        }
        [HttpGet("getAllUsers")]
        [Authorize(Roles = "GodMode")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var result = await _userService.GetAllUsersAsync();
            return HandleResult(result);
        }
        [HttpPut("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestDto request)
        {
            var phone = User.GetMobilePhone();
            var result = await _userService.ChangePasswordAsync(request, phone);
            return HandleResult(result);
        }
        [HttpPatch("updateUser")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfoAsync([FromBody] EditUserRequestDto request)
        {
            var phone = User.GetMobilePhone();
            var result = await _userService.UpdateUserAsync(request, phone);
            return HandleResult(result);
        }
    }
}
