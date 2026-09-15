using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Entities;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;

namespace StepanCarSevice.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase<AuthController>
    {
        private readonly IAuthService _authService;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        public AuthController(IAuthService authService,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            ILogger<AuthController> logger, 
            IErrorMapper errorMapper) : base (errorMapper, logger)
        {
            _accessor = accessor;
            _authService = authService;
        }

        [HttpPost("register")]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto model)
        {
            var result = await _authService.RegisterAsync(model);
            return HandleResult(result);
        }

        [HttpPost("login")]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto model)
        {
            var result = await _authService.LoginAsync(model);
            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("getTokenClaims")]
        public IActionResult GetClaimsAsync()
        {
            var result = _authService.GetClaims();
            return HandleResult(result);
        }
    }
}
