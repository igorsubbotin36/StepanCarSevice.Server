using System.Net;
using System.Security.Claims;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepanCarService.Common.API.AppExtensions;
using StepanCarService.Common.API.BuilderExtensions;
using StepanCarService.Common.Application.Interfaces;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DependencyInjection;
using StepanCarService.Common.Infastructure.Mappers;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.Common.Tests.Api;

// Минимальный сервис на общем пайплайне Common.API (AddSharedBulderSettings + UseSharedPipeline) без БД:
// тенанты — в памяти, вместо контроллеров — пробные эндпоинты. Проверяет JWT, мультитенантность, политики,
// rate limiting и проверки старта так же, как они работают в настоящих сервисах
public sealed class SharedPipelineHost : IAsyncDisposable
{
    public const string OwnerUserId = "7";
    // Адрес клиента для rate limiting (в TestServer удалённого адреса нет)
    public const string ClientIpHeader = "X-Test-Client-Ip";

    public static readonly TenantInfoEntity TenantA = Tenant("tenant-a", ownerUserId: 7);
    public static readonly TenantInfoEntity TenantB = Tenant("tenant-b", ownerUserId: 8);
    // Тенант, созданный GodMode: владельца нет
    public static readonly TenantInfoEntity TenantWithoutOwner = Tenant("tenant-c", ownerUserId: null);
    public static readonly TenantInfoEntity InactiveTenant = Tenant("tenant-inactive", ownerUserId: 7, isActive: false);

    public static readonly string[] Policies = ["GodModeOnly", "TenantOwnerInTenant", "TenantModeratorInTenant", "UserInTenant"];

    private readonly WebApplication _app;

    private SharedPipelineHost(WebApplication app)
    {
        _app = app;
    }

    public IServiceProvider Services => _app.Services;

    public static async Task<SharedPipelineHost> StartAsync(string environment = "Development", IDictionary<string, string?>? settings = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = environment,
            ApplicationName = typeof(SharedPipelineHost).Assembly.GetName().Name
        });
        builder.WebHost.UseTestServer();

        var configuration = new Dictionary<string, string?>
        {
            ["AllowedHosts"] = "localhost;*.localhost",
            ["Jwt:Key"] = TestTokenFactory.Key,
            ["Jwt:Issuer"] = TestTokenFactory.Issuer,
            ["Jwt:Audience"] = TestTokenFactory.Audience
        };
        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
            configuration[key] = value;
        builder.Configuration.AddInMemoryCollection(configuration);

        builder.AddSharedBulderSettings();
        builder.Services.AddSharedJwtAuthentication(builder.Configuration);
        builder.Services.AddScoped<IErrorMapper, ErrorMapper>();
        builder.Services.AddMultiTenant<TenantInfoEntity>()
            .WithHostStrategy()
            .WithInMemoryStore(options =>
            {
                foreach (var tenant in new[] { TenantA, TenantB, TenantWithoutOwner, InactiveTenant })
                    options.Tenants.Add(tenant);
            });

        var app = builder.Build();
        try
        {
            app.Use((context, next) =>
            {
                if (context.Request.Headers.TryGetValue(ClientIpHeader, out var ip))
                    context.Connection.RemoteIpAddress = IPAddress.Parse(ip.ToString());
                return next(context);
            });
            app.UseSharedPipeline(null!);
            MapProbeEndpoints(app);
            await app.StartAsync();
            return new SharedPipelineHost(app);
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    // Клиент к порталу (tenantIdentifier = null) или к поддомену тенанта
    public HttpClient CreateClient(string? tenantIdentifier = null, string domain = "localhost", string scheme = "http")
    {
        var client = _app.GetTestClient();
        client.BaseAddress = new Uri($"{scheme}://{(tenantIdentifier == null ? domain : $"{tenantIdentifier}.{domain}")}/");
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    private static void MapProbeEndpoints(WebApplication app)
    {
        app.MapGet("probe/anonymous", () => "ok");
        app.MapGet("probe/auth-limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Auth);
        app.MapGet("probe/public-limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Public);
        // tenant_id после OnTokenValidated: у GodMode и владельца он подставляется по тенанту запроса
        app.MapGet("probe/authenticated", (ClaimsPrincipal user) => new ProbeIdentity(user.FindFirst("tenant_id")?.Value))
            .RequireAuthorization();
        foreach (var policy in Policies)
            app.MapGet($"probe/policy/{policy}", () => "ok").RequireAuthorization(policy);
    }

    private static TenantInfoEntity Tenant(string identifier, int? ownerUserId, bool isActive = true)
    {
        var builder = TenantBuilder.Tenant().WithIdentifier(identifier);
        if (ownerUserId != null)
            builder.OwnedBy(ownerUserId.Value);
        if (!isActive)
            builder.Inactive();
        return builder.Build();
    }
}

public sealed record ProbeIdentity(string? TenantId);
