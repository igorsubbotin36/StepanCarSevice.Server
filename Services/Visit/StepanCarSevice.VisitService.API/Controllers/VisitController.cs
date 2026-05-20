using Microsoft.AspNetCore.Mvc;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;

namespace StepanCarSevice.VisitService.API.Controllers
{
    public class VisitController : ApiControllerBase<VisitController>
    {
        public VisitController(IErrorMapper errorMapper, ILogger<VisitController> logger) : base(errorMapper, logger)
        {
        }
    }
}
