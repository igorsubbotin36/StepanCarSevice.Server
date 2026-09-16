using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StepanCarService.Common.API.Controllers;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Infastructure.Mappers;

namespace StepanCarService.Common.Tests.Unit;

// Преобразование Result в HTTP-ответ (ApiControllerBase.HandleResult)
[Trait(TestCategories.Name, TestCategories.Unit)]
public class ApiControllerBaseTests
{
    [Fact]
    public void HandleResult_Success_Returns200WithoutBody()
    {
        var response = new ProbeController(new ErrorMapper()).Handle(Result.Success());

        response.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public void HandleResultOfT_SuccessWithNull_Returns204()
    {
        var response = new ProbeController(new ErrorMapper()).Handle(Result.Success<string?>(null));

        response.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void HandleResultOfT_SuccessWithValue_Returns200WithValue()
    {
        var response = new ProbeController(new ErrorMapper()).Handle(Result.Success(new[] { 1, 2 }));

        response.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(new[] { 1, 2 });
    }

    // Каждый статус из маппинга доходит до клиента вместе с кодом и текстом ошибки
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public void HandleResult_Failure_ReturnsMappedStatusAndErrorBody(HttpStatusCode status)
    {
        var mapper = Substitute.For<IErrorMapper>();
        mapper.Map("SOME_ERROR").Returns((status, "Текст ошибки"));
        var controller = new ProbeController(mapper);

        foreach (var response in new[] { controller.Handle(Result.Failure("SOME_ERROR")), controller.Handle(Result.Failure<int>("SOME_ERROR")) })
        {
            var result = response.ShouldBeAssignableTo<IStatusCodeActionResult>();
            result.StatusCode.ShouldBe((int)status);
            ReadError(response).ShouldBe(("SOME_ERROR", "Текст ошибки"));
        }
    }

    [Fact]
    public void HandleResult_UnknownErrorCode_Returns500WithOriginalCode()
    {
        var response = new ProbeController(new ErrorMapper()).Handle(Result.Failure("NO_SUCH_CODE"));

        response.ShouldBeAssignableTo<IStatusCodeActionResult>().StatusCode.ShouldBe(500);
        ReadError(response).ShouldBe(("NO_SUCH_CODE", "Произошла непредвиденная ошибка"));
    }

    // Тело ошибки — анонимный объект: читаем так же, как его увидит клиент (JSON)
    private static (string? ErrorCode, string? ErrorText) ReadError(IActionResult response)
    {
        var value = response.ShouldBeAssignableTo<ObjectResult>().Value;
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value));
        return (json.RootElement.GetProperty("errorCode").GetString(), json.RootElement.GetProperty("errorText").GetString());
    }

    // Не public: не должен попасть в контроллеры тестовых хостов
    private sealed class ProbeController(IErrorMapper errorMapper)
        : ApiControllerBase<ProbeController>(errorMapper, NullLogger<ProbeController>.Instance)
    {
        public IActionResult Handle(Result result) => HandleResult(result);
        public IActionResult Handle<T>(Result<T> result) => HandleResult(result);
    }
}
