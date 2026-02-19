using StepanCarService.Core.Interfaces;
using StepanCarService.Web.Controllers;

namespace StepanCarSevice.DetailService.API.Controllers
{
    public class DetailController : ApiControllerBase<DetailController>
    {
        public DetailController(IErrorMapper errorMapper, ILogger<DetailController> logger) : base(errorMapper, logger)
        {
        }
    }
}
