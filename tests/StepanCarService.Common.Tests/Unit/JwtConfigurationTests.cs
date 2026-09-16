using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Infastructure.Auth;
using StepanCarService.Common.Infastructure.DependencyInjection;

namespace StepanCarService.Common.Tests.Unit;

// Проверка настроек JWT при старте сервиса (AddSharedJwtAuthentication) и состав политик авторизации
[Trait(TestCategories.Name, TestCategories.Unit)]
public class JwtConfigurationTests
{
    private const string ValidKey = "0123456789abcdef0123456789abcdef";

    private static IServiceCollection AddJwt(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddSharedJwtAuthentication(configuration);
    }

    private static Dictionary<string, string?> ValidSettings(string key = ValidKey, string lifetime = "15") => new()
    {
        ["Jwt:Key"] = key,
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience",
        ["Jwt:LifetimeMinutes"] = lifetime
    };

    [Fact]
    public void MissingJwtSection_Throws()
    {
        Should.Throw<InvalidOperationException>(() => AddJwt([]))
            .Message.ShouldContain("JWT configuration section is missing");
    }

    [Theory]
    [InlineData("Jwt:Key", "", "JWT Key is not configured")]
    [InlineData("Jwt:Key", "   ", "JWT Key is not configured")]
    [InlineData("Jwt:Issuer", "", "JWT Issuer is not configured")]
    [InlineData("Jwt:Audience", " ", "JWT Audience is not configured")]
    public void EmptyRequiredValue_ThrowsWithClearMessage(string setting, string value, string expectedMessage)
    {
        var settings = ValidSettings();
        settings[setting] = value;

        Should.Throw<InvalidOperationException>(() => AddJwt(settings)).Message.ShouldContain(expectedMessage);
    }

    // Значение-заглушка из appsettings.json не должно позволить запустить сервис
    [Theory]
    [InlineData("your_private_key")]
    [InlineData("0123456789abcdef0123456789abcde")]
    public void KeyShorterThan32Bytes_Throws(string key)
    {
        Should.Throw<InvalidOperationException>(() => AddJwt(ValidSettings(key)))
            .Message.ShouldContain("JWT Key is too short");
    }

    // Длина считается в байтах UTF-8 — 16 кириллических букв дают 32 байта
    [Theory]
    [InlineData(ValidKey)]
    [InlineData("абвгдеёжзийклмно")]
    public void KeyOf32Bytes_IsAccepted(string key)
    {
        Should.NotThrow(() => AddJwt(ValidSettings(key)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void NonPositiveLifetime_DefaultsTo60Minutes(string lifetime)
    {
        using var provider = AddJwt(ValidSettings(lifetime: lifetime)).BuildServiceProvider();

        provider.GetRequiredService<JwtOptions>().LifetimeMinutes.ShouldBe(60);
    }

    // Роли в политиках — только существующие константы Roles, без опечаток
    [Fact]
    public void Policies_UseOnlyKnownRoles()
    {
        using var provider = AddJwt(ValidSettings()).AddLogging().BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        string[] knownRoles = [Roles.GodMode, Roles.TenantOwner, Roles.TenantModerator, Roles.User];

        var policyRoles = new[] { "GodModeOnly", "TenantOwnerInTenant", "TenantModeratorInTenant", "UserInTenant" }
            .Select(name => options.GetPolicy(name).ShouldNotBeNull($"политика {name} не зарегистрирована"))
            .SelectMany(policy => policy.Requirements)
            .SelectMany(requirement => requirement switch
            {
                TenantRoleRequirement tenantRole => tenantRole.Roles,
                RolesAuthorizationRequirement roles => roles.AllowedRoles,
                _ => []
            })
            .Distinct()
            .ToList();

        policyRoles.ShouldNotBeEmpty();
        policyRoles.ShouldAllBe(role => knownRoles.Contains(role));
        new[] { Roles.GodMode, Roles.TenantOwner, Roles.TenantModerator, Roles.User }
            .ShouldBe(["GodMode", "TenantOwner", "TenantModerator", "User"]);
    }
}
