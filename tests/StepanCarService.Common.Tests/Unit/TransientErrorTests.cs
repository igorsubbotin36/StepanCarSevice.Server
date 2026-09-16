using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StepanCarService.Common.Infastructure.Messaging;

namespace StepanCarService.Common.Tests.Unit;

// Классификация ошибок consumer'а: временные повторяются без ограничения, остальные уходят в .dead
[Trait(TestCategories.Name, TestCategories.Unit)]
public class TransientErrorTests
{
    [Fact]
    public void IsTransient_NpgsqlSocketError_ReturnsTrue()
    {
        var exception = new NpgsqlException("Connection refused", new SocketException((int)SocketError.ConnectionRefused));

        TenantEventsConsumer.IsTransient(exception).ShouldBeTrue();
    }

    [Fact]
    public void IsTransient_TimeoutWrappedInDbUpdateException_ReturnsTrue()
    {
        var exception = new DbUpdateException("Save failed", new TimeoutException());

        TenantEventsConsumer.IsTransient(exception).ShouldBeTrue();
    }

    [Fact]
    public void IsTransient_InvalidOperation_ReturnsFalse()
    {
        TenantEventsConsumer.IsTransient(new InvalidOperationException()).ShouldBeFalse();
    }

    [Fact]
    public void IsTransient_UniqueViolationWrappedInDbUpdateException_ReturnsFalse()
    {
        var postgresException = new PostgresException("duplicate key value violates unique constraint", "ERROR", "ERROR", "23505");
        var exception = new DbUpdateException("Save failed", postgresException);

        TenantEventsConsumer.IsTransient(exception).ShouldBeFalse();
    }
}
