using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarSevice.DetailService.Application.Interfaces.Services;
using StepanCarSevice.DetailService.Application.Models.DTO;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetailController : ApiControllerBase<DetailController>
    {
        private readonly IDetailService _detailService;
        public DetailController(IDetailService detailService, 
            IErrorMapper errorMapper, 
            ILogger<DetailController> logger) : base(errorMapper, logger)
        {
            _detailService = detailService;
        }

        [HttpPost("addDetail")]
        [Authorize(Policy = "TenantModeratorInTenant")]
        public async Task<IActionResult> AddDetail(DetailCreateDto detail)
        {
            var result = await _detailService.AddASync(detail);
            return HandleResult(result);
        }

        [HttpGet("getAllDetails")]
        [Authorize(Policy = "TenantModeratorInTenant")]
        public async Task<IActionResult> GetAllDetailsAsync()
        {
            var result = await _detailService.GetAllDetailsAsync();
            return HandleResult(result);
        }

        [HttpGet("getDetailById")]
        [Authorize(Roles = "TenantModeratorInTenant")]
        public async Task<IActionResult> GetDetailByIdAsync([FromBody] int id)
        {
            var result = await _detailService.GetDetailByIdAsync(id);
            return HandleResult(result);
        }

        [HttpGet("getDetailsByCode")]
        [Authorize(Roles = "TenantModeratorInTenant")]
        public async Task<IActionResult> GetDetailsByCodeAsync([FromBody] string code)
        {
            var result = await _detailService.GetDetailsByCodeAsync(code);
            return HandleResult(result);
        }

        [HttpPatch("editDetail")]
        [Authorize(Roles = "TenantModeratorInTenant")]
        public async Task<IActionResult> EditDetailAsync([FromBody] DetailUpdateDto detail)
        {
            var result = await _detailService.EditDetailAsync(detail);
            return HandleResult(result);
        }
    }
}
