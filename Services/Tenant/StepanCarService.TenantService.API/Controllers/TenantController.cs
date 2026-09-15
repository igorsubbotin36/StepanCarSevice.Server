using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Models.DTOs;
using System.Security.Claims;

namespace StepanCarService.TenantService.API.Controllers;

// Управление тенантами — функция портала: доступно только GodMode и владельцам (TenantOwner)
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.TenantOwner}, {Roles.GodMode}")]
public class TenantController : ApiControllerBase<TenantController>
{
    private readonly ITenantService _tenantService;

    public TenantController(IErrorMapper errorMapper, ILogger<TenantController> logger, ITenantService tenantService)
        : base(errorMapper, logger)
    {
        _tenantService = tenantService;
    }

    [HttpPost("registerTenant")]
    public async Task<IActionResult> RegisterTenantAsync([FromBody] TenantCreateDto tenantDto)
    {
        var result = await _tenantService.AddAsync(tenantDto, GetCaller());
        return HandleResult(result);
    }
    [HttpPatch("updateTenant")]
    public async Task<IActionResult> UpdateTenantAsync([FromBody] TenantUpdateDto tenantDto)
    {
        var result = await _tenantService.UpdateAsync(tenantDto, GetCaller());
        return HandleResult(result);
    }
    [HttpDelete("deleteTenant")]
    public async Task<IActionResult> DeleteTenantAsync(string id)
    {
        var result = await _tenantService.DeleteAsync(id, GetCaller());
        return HandleResult(result);
    }
    [HttpGet("getAllTenants")]
    [Authorize(Roles = Roles.GodMode)]
    public async Task<IActionResult> GetAllTenantsAsync()
    {
        var result = await _tenantService.GetAllAsync();
        return HandleResult(result);
    }
    [HttpGet("getTenantById")]
    public async Task<IActionResult> GetTenantByIdAsync(string id)
    {
        var result = await _tenantService.GetByIdAsync(id, GetCaller());
        return HandleResult(result);
    }
    // Публичная страница портала «Подключённые автосервисы»: доступна без регистрации
    [HttpGet("getConnectedTenants")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Public)]
    public async Task<IActionResult> GetConnectedTenantsAsync()
    {
        var result = await _tenantService.GetConnectedTenantsAsync();
        return HandleResult(result);
    }
    [HttpGet("getMyTenant")]
    [Authorize(Roles = Roles.TenantOwner)]
    public async Task<IActionResult> GetMyTenantAsync()
    {
        var result = await _tenantService.GetMyTenantAsync(GetCaller());
        return HandleResult(result);
    }

    private TenantCaller GetCaller()
    {
        var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (int?)null;
        return new TenantCaller(userId, User.IsInRole(Roles.GodMode));
    }
}
