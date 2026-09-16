namespace StepanCarService.TestKit.MultiTenancy;

// Хосты запросов: тенант определяется по поддомену (Finbuckle Host Strategy).
// *.localhost разрешён в AllowedHosts окружения Development
public static class TenantHost
{
    public const string Portal = "localhost";

    public static string For(string tenantIdentifier) => $"{tenantIdentifier}.{Portal}";

    public static Uri PortalAddress { get; } = new($"http://{Portal}/");

    public static Uri AddressFor(string tenantIdentifier) => new($"http://{For(tenantIdentifier)}/");
}
