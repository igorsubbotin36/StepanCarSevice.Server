using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.TestKit.Http;

namespace StepanCarService.Common.Tests.Api;

// Общий пайплайн сервисов (UseSharedPipeline, AddSharedBulderSettings): проверки старта, host filtering,
// OpenAPI, HSTS и rate limiting. Каждый тест поднимает свой хост — лимиты и настройки не пересекаются
[Trait(TestCategories.Name, TestCategories.Api)]
public class SharedPipelineTests
{
    private const string Production = "Production";
    private const string ProductionHosts = "example.com;*.example.com";

    private static Dictionary<string, string?> Settings(params (string Key, string? Value)[] values) =>
        values.ToDictionary(v => v.Key, v => v.Value);

    // Тенант определяется по Host, поэтому в проде список хостов обязателен и без «*»
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("example.com;*")]
    public async Task Production_WithoutExplicitAllowedHosts_FailsToStart(string? allowedHosts)
    {
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            SharedPipelineHost.StartAsync(Production, Settings(("AllowedHosts", allowedHosts))));

        exception.Message.ShouldContain("AllowedHosts");
    }

    [Fact]
    public async Task Production_WithExplicitAllowedHosts_Starts()
    {
        await using var host = await SharedPipelineHost.StartAsync(Production, Settings(("AllowedHosts", ProductionHosts)));
        using var client = host.CreateClient(domain: "example.com");

        var response = await client.GetAsync("probe/anonymous");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Development_DoesNotRequireAllowedHosts()
    {
        await using var host = await SharedPipelineHost.StartAsync(settings: Settings(("AllowedHosts", "*")));
        using var client = host.CreateClient();

        await (await client.GetAsync("probe/anonymous")).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Документация API только в Development, со схемой Bearer
    [Fact]
    public async Task Development_ServesOpenApiAndSwaggerWithBearerScheme()
    {
        await using var host = await SharedPipelineHost.StartAsync();
        using var client = host.CreateClient();

        await (await client.GetAsync("openapi/v1.json")).ShouldBeStatusAsync(HttpStatusCode.OK);
        await (await client.GetAsync("index.html")).ShouldBeStatusAsync(HttpStatusCode.OK);
        var swagger = await client.GetAsync("swagger/v1/swagger.json");
        await swagger.ShouldBeStatusAsync(HttpStatusCode.OK);
        (await swagger.Content.ReadAsStringAsync()).ShouldContain("\"Bearer\"");
    }

    [Fact]
    public async Task Production_DoesNotServeOpenApiOrSwagger()
    {
        await using var host = await SharedPipelineHost.StartAsync(Production, Settings(("AllowedHosts", ProductionHosts)));
        using var client = host.CreateClient(domain: "example.com");

        (await client.GetAsync("openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await client.GetAsync("swagger/v1/swagger.json")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await client.GetAsync("index.html")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Production_HttpsResponse_HasHstsHeader()
    {
        await using var host = await SharedPipelineHost.StartAsync(Production, Settings(("AllowedHosts", ProductionHosts)));
        using var client = host.CreateClient(domain: "example.com", scheme: "https");

        var response = await client.GetAsync("probe/anonymous");

        await response.ShouldBeStatusAsync(HttpStatusCode.OK);
        response.Headers.Contains("Strict-Transport-Security").ShouldBeTrue();
    }

    // Запрос с хостом не из AllowedHosts не доходит до определения тенанта
    [Theory]
    [InlineData("evil.example.com")]
    [InlineData("tenant-a.localhost.evil.com")]
    public async Task HostNotInAllowedHosts_Returns400(string hostName)
    {
        await using var host = await SharedPipelineHost.StartAsync();
        using var client = host.CreateClient(domain: hostName);

        (await client.GetAsync("probe/anonymous")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // Политика по атрибуту эндпоинта срабатывает (лимитер стоит после routing)
    [Fact]
    public async Task AuthPolicy_Default_Allows10RequestsPerWindow()
    {
        await using var host = await SharedPipelineHost.StartAsync();
        using var client = host.CreateClient();

        for (var i = 0; i < 10; i++)
            await (await client.GetAsync("probe/auth-limited")).ShouldBeStatusAsync(HttpStatusCode.OK);

        (await client.GetAsync("probe/auth-limited")).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task PublicPolicy_Default_Allows60RequestsPerWindow()
    {
        await using var host = await SharedPipelineHost.StartAsync();
        using var client = host.CreateClient();

        for (var i = 0; i < 60; i++)
            await (await client.GetAsync("probe/public-limited")).ShouldBeStatusAsync(HttpStatusCode.OK);

        (await client.GetAsync("probe/public-limited")).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task AuthPolicy_UsesConfiguredLimitAndWindow()
    {
        await using var host = await SharedPipelineHost.StartAsync(settings: Settings(
            ("RateLimiting:Auth:PermitLimit", "2"), ("RateLimiting:Auth:WindowSeconds", "1")));
        using var client = host.CreateClient();

        await (await client.GetAsync("probe/auth-limited")).ShouldBeStatusAsync(HttpStatusCode.OK);
        await (await client.GetAsync("probe/auth-limited")).ShouldBeStatusAsync(HttpStatusCode.OK);
        (await client.GetAsync("probe/auth-limited")).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        // Окно в 1 секунду: лимит восстанавливается
        await Eventually.WaitUntilAsync(async () => (await client.GetAsync("probe/auth-limited")).IsSuccessStatusCode,
            TimeSpan.FromSeconds(5), because: "после окна лимит должен восстановиться");
    }

    [Fact]
    public async Task RejectedRequest_HasErrorBodyAndRetryAfter()
    {
        await using var host = await SharedPipelineHost.StartAsync(settings: Settings(("RateLimiting:Auth:PermitLimit", "1")));
        using var client = host.CreateClient();
        await client.GetAsync("probe/auth-limited");

        var response = await client.GetAsync("probe/auth-limited");

        await response.ShouldBeErrorAsync(HttpStatusCode.TooManyRequests, SystemErrors.TooManyRequests);
        response.Headers.RetryAfter.ShouldNotBeNull();
    }

    [Fact]
    public async Task Limits_AreCountedPerClientIp()
    {
        await using var host = await SharedPipelineHost.StartAsync(settings: Settings(("RateLimiting:Auth:PermitLimit", "1")));
        using var client = host.CreateClient();

        async Task<HttpStatusCode> SendFrom(string ip)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "probe/auth-limited");
            request.Headers.Add(SharedPipelineHost.ClientIpHeader, ip);
            return (await client.SendAsync(request)).StatusCode;
        }

        (await SendFrom("10.0.0.1")).ShouldBe(HttpStatusCode.OK);
        (await SendFrom("10.0.0.1")).ShouldBe(HttpStatusCode.TooManyRequests);
        (await SendFrom("10.0.0.2")).ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointWithoutLimitAttribute_IsNotLimited()
    {
        await using var host = await SharedPipelineHost.StartAsync(settings: Settings(
            ("RateLimiting:Auth:PermitLimit", "1"), ("RateLimiting:Public:PermitLimit", "1")));
        using var client = host.CreateClient();

        for (var i = 0; i < 20; i++)
            await (await client.GetAsync("probe/anonymous")).ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Стандартные провайдеры логирования заменены NLog
    [Fact]
    public async Task Logging_UsesNLogOnly()
    {
        await using var host = await SharedPipelineHost.StartAsync();

        var providers = host.Services.GetServices<ILoggerProvider>().Select(p => p.GetType().Name).ToList();

        providers.ShouldContain(name => name.Contains("NLog"));
        providers.ShouldNotContain(name => name.Contains("Console"));
    }
}
