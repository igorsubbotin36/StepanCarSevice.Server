using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarService.Core.Interfaces;
using StepanCarService.Web.Controllers;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase<AuthController>
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService, ILogger<AuthController> logger, IErrorMapper errorMapper) : base (errorMapper, logger)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto model)
        {
            var result = await _userService.RegisterAsync(model);
            return HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto model)
        {
            var result = await _userService.LoginAsync(model);
            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("getTokenClaims")]
        public IActionResult GetClaimsAsync()
        {
            var result = _userService.GetClaims();
            return HandleResult(result);
        }
    }
}
