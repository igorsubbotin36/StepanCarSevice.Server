using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Extensions;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Controllers;
using StepanCarSevice.AuthService.Domain.Repository;
using StepanCarSevice.AuthService.Infrastructure.Auth;

namespace StepanCarSevice.AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        public UserController(IUserService userService, ILogger<AuthController> logger, IErrorMapper errorMapper) : base(errorMapper)
        {
            _userService = userService;
            _logger = logger;
        }
        [HttpGet("getUserInfo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserInfoAsync(string phone)
        {
            var result = await _userService.GetUserInfoByPhoneAsync(phone);
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
