using Microsoft.AspNetCore.Mvc;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;
using System.Net;

namespace StepanCarSevice.AuthService.API.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly IErrorMapper _errorMapper;

        protected ApiControllerBase(IErrorMapper errorMapper)
        {
            _errorMapper = errorMapper;
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok(); // или NoContent() если нужно 204

            return HandleFailure(result);
        }

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Value == null
                    ? NoContent()
                    : Ok(result.Value);
            }

            return HandleFailure(result);
        }

        private IActionResult HandleFailure(Result result)
        {
            var (statusCode, message) = _errorMapper.Map(result.ErrorCode);

            // Используем более понятное сообщение
            var response = new
            {
                errorCode = result.ErrorCode,
                errorText = message
            };

            return statusCode switch
            {
                HttpStatusCode.Unauthorized => Unauthorized(response),
                HttpStatusCode.Forbidden => StatusCode(403, response),
                HttpStatusCode.NotFound => NotFound(response),
                HttpStatusCode.BadRequest => BadRequest(response),
                HttpStatusCode.Conflict => Conflict(response),
                HttpStatusCode.UnprocessableEntity => StatusCode(422, response),
                HttpStatusCode.InternalServerError => StatusCode(500, response),
                HttpStatusCode.ServiceUnavailable => StatusCode(503, response),
                _ => StatusCode((int)statusCode, response)
            };
        }
    }
}
