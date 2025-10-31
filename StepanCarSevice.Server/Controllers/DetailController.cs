using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StepanCarSevice.Domain.Entities;
using StepanCarSevice.Application.Repository.Interfaces;
using System.Formats.Asn1;

namespace StepanCarSevice.Server.Controllers
{
    public class DetailController : Controller
    {
        private readonly ILogger<DetailController> _logger;
        private readonly IDetailRepository _rep;
        public DetailController(ILogger<DetailController> logger, IDetailRepository rep)
        {
            _logger = logger;
            _rep = rep;
        }

        [HttpGet("/getAllDetails")]
        [Authorize(Roles = "admin")]
        public async Task<List<Detail>> GetAllDetails()
        {
            return await _rep.GetAllDetails();
        }

        [HttpPatch("/editDetail")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditDetail(Detail detail)
        {
            if (await _rep.EditDetail(detail))
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { errorText = "Unable to edit detail" });
            }
        }

        [HttpDelete("/deleteDetail")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteDetails(int[] ids)
        {
            if (await _rep.DeleteDetails(ids))
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { errorText = "Unable to delete details" });
            }
        }

        [HttpPost("/addDetail")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddDetail(Detail detail)
        {
            if (await _rep.AddDetail(detail))
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { errorText = "Unable to add details" });
            }
        }
    }
}
