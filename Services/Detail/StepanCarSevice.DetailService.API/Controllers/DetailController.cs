using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using StepanCarService.Core.Interfaces;
using StepanCarService.Web.Controllers;
using StepanCarSevice.DetailService.Application.Interfaces;
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

        [HttpGet("getAllDetails")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDetailsAsync()
        {
            var result = await _detailService.GetAllDetailsAsync();
            return HandleResult(result);
        }

        [HttpGet("getDetailById")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetailByIdAsync([FromBody] int id)
        {
            var result = await _detailService.GetDetailByIdAsync(id);
            return HandleResult(result);
        }

        [HttpGet("getDetailsByCode")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetailsByCodeAsync([FromBody] string code)
        {
            var result = await _detailService.GetDetailsByCodeAsync(code);
            return HandleResult(result);
        }

        [HttpPatch("editDetail")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDetailAsync([FromBody] Detail detail)
        {
            var result = await _detailService.EditDetailAsync(detail);
            return HandleResult(result);
        }
    }
}
