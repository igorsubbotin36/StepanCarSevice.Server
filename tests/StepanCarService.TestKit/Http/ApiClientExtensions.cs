using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StepanCarService.TestKit.Http;

// Тело ошибки API (ApiControllerBase.HandleResult и 429 от rate limiter)
public sealed record ApiError(string? ErrorCode, string? ErrorText);

public static class ApiClientExtensions
{
    public static HttpClient WithBearer(this HttpClient client, string? token)
    {
        client.DefaultRequestHeaders.Authorization = token == null ? null : new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<ApiError> ReadErrorAsync(this HttpResponseMessage response)
    {
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        return error ?? new ApiError(null, null);
    }

    // Проверка ошибки API: и HTTP-статус, и errorCode из тела
    public static async Task ShouldBeErrorAsync(this HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(status, $"тело ответа: {body}");
        var error = System.Text.Json.JsonSerializer.Deserialize<ApiError>(body,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        error.ShouldNotBeNull().ErrorCode.ShouldBe(errorCode);
    }

    public static async Task ShouldBeStatusAsync(this HttpResponseMessage response, HttpStatusCode status)
    {
        if (response.StatusCode != status)
        {
            var body = await response.Content.ReadAsStringAsync();
            response.StatusCode.ShouldBe(status, $"тело ответа: {body}");
        }
    }
}
