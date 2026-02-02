using Microsoft.AspNetCore.Mvc;
using StepanCarSevice.AuthService.API.Controllers;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Repository;
using StepanCarSevice.AuthService.Infrastructure.Auth;

namespace StepanCarSevice.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IUserService userService, IUserRepository userRepo, IPasswordHasher passwordHasher, JwtOptions jwtOptions, ILogger<AuthController> logger, IErrorMapper errorMapper) : base (errorMapper)
        {
            _userService = userService;
            _logger = logger;
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

        [HttpGet("getTokenClaims")]
        public IActionResult GetClaimsAsync(string token)
        {
            var result = _userService.GetClaims(token);
            return HandleResult(result);
        }
    }
}
