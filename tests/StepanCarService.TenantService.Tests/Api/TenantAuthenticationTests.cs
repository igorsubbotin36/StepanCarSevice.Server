using System.Net;
using System.Net.Http.Json;
using StepanCarService.Common.Application.Models;
using StepanCarService.TenantService.Application.Models.DTOs;
using StepanCarService.TenantService.Infrastructure.DbContexts;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Hosting;
using StepanCarService.TestKit.Http;
using StepanCarService.TestKit.Jwt;

namespace StepanCarService.TenantService.Tests.Api;

// Проверка токенов в Tenant-сервисе: управление тенантами — только с портала
[Trait(TestCategories.Name, TestCategories.Api)]
[Collection(TestCollections.Database)]
public class TenantAuthenticationTests(TenantServiceFactory factory)
    : ApiTestBase<TenantServiceFactory, Program, TenantServiceDbContext>(factory)
{
    // У Tenant-сервиса статическая стратегия "management" без тенанта в БД — любой токен с tenant_id отклоняется
    [Theory]
    [InlineData(Roles.TenantOwner)]
    [InlineData(Roles.GodMode)]
    [InlineData(Roles.User)]
    public async Task TokenWithTenantId_Returns401(string role)
    {
        var tenant = await Factory.WithDbContextAsync(async db =>
        {
            var entity = TenantBuilder.Tenant().OwnedBy(7).Build();
            db.Tenants.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        });
        using var client = Factory.CreateClientFor().WithBearer(TestTokenFactory.Create(role, userId: 7, tenantId: tenant.Id));

        (await client.GetAsync("api/Tenant/getAllTenants")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.GetAsync($"api/Tenant/getTenantById?id={tenant.Id}")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("api/Tenant/registerTenant", new TenantCreateDto(TestData.TenantIdentifier(), "Сервис")))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Поддомен в запросе ничего не меняет — стратегия статическая
    [Fact]
    public async Task RequestToSubdomain_IsStillPortal()
    {
        var tenant = await Factory.WithDbContextAsync(async db =>
        {
            var entity = TenantBuilder.Tenant().OwnedBy(7).Build();
            db.Tenants.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        });
        using var client = Factory.CreateClientFor(tenant.Identifier)
            .WithBearer(TestTokenFactory.Create(Roles.TenantOwner, userId: 7, tenantId: tenant.Id));

        (await client.GetAsync("api/Tenant/getMyTenant")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await (await client.WithBearer(TestTokenFactory.Create(Roles.TenantOwner, userId: 7)).GetAsync("api/Tenant/getMyTenant"))
            .ShouldBeStatusAsync(HttpStatusCode.OK);
    }

    // Известное ограничение — security_stamp проверяется только в Auth; Tenant принимает токен до истечения срока
    [Fact]
    public async Task TokenWithStaleSecurityStamp_IsAcceptedUntilExpiration()
    {
        using var client = Factory.CreateClientFor()
            .WithBearer(TestTokenFactory.Create(Roles.GodMode, userId: 1, securityStamp: "revoked-long-ago"));

        await (await client.GetAsync("api/Tenant/getAllTenants")).ShouldBeStatusAsync(HttpStatusCode.OK);
    }
}
