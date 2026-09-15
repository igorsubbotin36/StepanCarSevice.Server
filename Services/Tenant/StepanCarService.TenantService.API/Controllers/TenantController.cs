using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Interfaces;
using StepanCarService.TenantService.Application.Models.DTOs;

namespace StepanCarService.TenantService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ApiControllerBase<TenantController>
{
    private readonly ITenantService _tenantService;

    public TenantController(IErrorMapper errorMapper, ILogger<TenantController> logger, ITenantService tenantService)
        : base(errorMapper, logger)
    {
        _tenantService = tenantService;
    }

    [HttpPost("registerTenant")]
    [Authorize(Roles = "TenantOwner, GodMode")]
    public async Task<IActionResult> RegisterTenantAsync([FromBody] TenantCreateDto tenantDto)
    {
        var result = await _tenantService.AddAsync(tenantDto);
        return HandleResult(result);
    }
    [HttpPatch("updateTenant")]
    [Authorize(Roles = "TenantOwner, GodMode")]
    public async Task<IActionResult> UpdateTenantAsync([FromBody] TenantUpdateDto tenantDto)
    {
        if (!HasAccessToTenant(tenantDto.Id))
            return HandleResult(Result.Failure(AuthErrors.Forbidden));
        var result = await _tenantService.UpdateAsync(tenantDto);
        return HandleResult(result);
    }
    [HttpDelete("deleteTenant")]
    [Authorize(Roles = "TenantOwner, GodMode")]
    public async Task<IActionResult> DeleteTenantAsync(string id)
    {
        if (!HasAccessToTenant(id))
            return HandleResult(Result.Failure(AuthErrors.Forbidden));
        var result = await _tenantService.DeleteAsync(id);
        return HandleResult(result);
    }
    [HttpGet("getAllTenants")]
    [Authorize(Roles = "GodMode")]
    public async Task<IActionResult> GetAllTenantsAsync()
    {
        var result = await _tenantService.GetAllAsync();
        return HandleResult(result);
    }
    [HttpGet("getTenantById")]
    [Authorize]
    public async Task<IActionResult> GetTenantByIdAsync(string id)
    {
        if (!HasAccessToTenant(id))
            return HandleResult(Result.Failure(AuthErrors.Forbidden));
        var result = await _tenantService.GetByIdAsync(id);
        return HandleResult(result);
    }

    // Временное правило до появления владельца тенанта (этап 2):
    // GodMode — к любому тенанту, остальные — только к тенанту из своего токена
    private bool HasAccessToTenant(string? tenantId)
    {
        if (User.IsInRole("GodMode"))
            return true;
        var tokenTenantId = User.FindFirst("tenant_id")?.Value;
        return !string.IsNullOrEmpty(tenantId)
            && string.Equals(tokenTenantId, tenantId, StringComparison.Ordinal);
    }
}
