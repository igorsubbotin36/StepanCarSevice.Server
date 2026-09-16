using Microsoft.EntityFrameworkCore;
using Npgsql;
using StepanCarService.Common.Core.Exceptions;
using StepanCarService.Common.Infastructure.Repositories;
using StepanCarService.TestKit.Builders;
using StepanCarService.TestKit.Databases;

namespace StepanCarService.Common.Tests.Integration;

[Trait(TestCategories.Name, TestCategories.Integration)]
[Collection(TestCollections.Database)]
public class UnitsOfWorkTests(CommonTestDatabase database) : DatabaseTestBase<CommonTestDatabase>(database)
{
    private async Task<bool> TenantExistsAsync(string id)
    {
        await using var check = Database.CreateContext();
        return await check.Tenants.AnyAsync(t => t.Id == id);
    }

    [Fact]
    public async Task Commit_SavesChanges()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(tenant);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
        }

        (await TenantExistsAsync(tenant.Id!)).ShouldBeTrue();
    }

    [Fact]
    public async Task Rollback_DiscardsSavedChanges()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(tenant);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.RollBackTransactionAsync();
        }

        (await TenantExistsAsync(tenant.Id!)).ShouldBeFalse();
    }

    // Сервисы отличают гонку на уникальном индексе (409) от прочих ошибок БД
    [Fact]
    public async Task SaveChanges_UniqueViolation_ThrowsUniqueConstraintViolation()
    {
        var identifier = TestData.TenantIdentifier();
        await using (var db = Database.CreateContext())
        {
            db.Tenants.Add(TenantBuilder.Tenant().WithIdentifier(identifier));
            await db.SaveChangesAsync();
        }

        await using var second = Database.CreateContext();
        second.Tenants.Add(TenantBuilder.Tenant().WithIdentifier(identifier));
        var exception = await Should.ThrowAsync<UniqueConstraintViolationException>(
            () => new UnitsOfWork<CommonTestDbContext>(second).SaveChangesAsync());

        exception.InnerException.ShouldBeOfType<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_OtherDatabaseError_IsRethrownAsIs()
    {
        await using var db = Database.CreateContext();
        db.Tenants.Add(TenantBuilder.Tenant().WithName(CommonTestDbContext.RejectedTenantName));

        var exception = await Should.ThrowAsync<DbUpdateException>(() => new UnitsOfWork<CommonTestDbContext>(db).SaveChangesAsync());

        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Rollback_WithoutTransactionOrAfterCommit_DoesNothing()
    {
        await using var db = Database.CreateContext();
        var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);

        await Should.NotThrowAsync(() => unitOfWork.RollBackTransactionAsync());
        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.CommitTransactionAsync();
        await Should.NotThrowAsync(() => unitOfWork.RollBackTransactionAsync());
        await Should.NotThrowAsync(() => unitOfWork.CommitTransactionAsync());
    }

    [Fact]
    public async Task SecondTransactionInSameScope_AfterCommit_Works()
    {
        var first = TenantBuilder.Tenant().Build();
        var second = TenantBuilder.Tenant().Build();
        await using (var db = Database.CreateContext())
        {
            var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(first);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();

            await unitOfWork.BeginTransactionAsync();
            db.Tenants.Add(second);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
        }

        (await TenantExistsAsync(first.Id!)).ShouldBeTrue();
        (await TenantExistsAsync(second.Id!)).ShouldBeTrue();
    }

    // Обычный шаблон сервисов — catch { Rollback } после упавшего Commit
    [Fact]
    public async Task Rollback_AfterFailedCommit_DoesNotThrow()
    {
        var tenant = TenantBuilder.Tenant().Build();
        await using var db = Database.CreateContext();
        var unitOfWork = new UnitsOfWork<CommonTestDbContext>(db);
        await unitOfWork.BeginTransactionAsync();
        db.Tenants.Add(tenant);
        await unitOfWork.SaveChangesAsync();
        // Соединение обрывается сервером посреди транзакции
        var pid = await db.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync();
        await using (var admin = new NpgsqlConnection(Database.ConnectionString))
        {
            await admin.OpenAsync();
            await using var terminate = new NpgsqlCommand($"SELECT pg_terminate_backend({pid})", admin);
            await terminate.ExecuteScalarAsync();
        }

        await Should.ThrowAsync<Exception>(() => unitOfWork.CommitTransactionAsync());
        await Should.NotThrowAsync(() => unitOfWork.RollBackTransactionAsync());
        (await TenantExistsAsync(tenant.Id!)).ShouldBeFalse();
    }
}
