using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
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
    public async Task<IActionResult> RegisterTenantAsync([FromBody] TenantDto tenantDto)
    {
        var result = await _tenantService.AddAsync(tenantDto);
        return HandleResult(result);
    }
    [HttpPatch("updateTenant")]
    [Authorize(Roles = "TenantOwner, GodMode")]
    public async Task<IActionResult> UpdateTenantAsync([FromBody] TenantDto tenantDto)
    {
        var result = await _tenantService.UpdateAsync(tenantDto);
        return HandleResult(result);
    }
    [HttpDelete("deleteTenant")]
    [Authorize(Roles = "TenantOwner, GodMode")]
    public async Task<IActionResult> DeleteTenantAsync(string id)
    {
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
    public async Task<IActionResult> GetTenantByIdAsync(string id)
    {
        var result = await _tenantService.GetByIdAsync(id);
        return HandleResult(result);
    }
}