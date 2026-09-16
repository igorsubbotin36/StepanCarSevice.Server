using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StepanCarService.Common.Infastructure.Messaging;

namespace StepanCarService.Common.Tests.Unit;

// Классификация ошибок consumer'а: временные повторяются без ограничения, остальные уходят в .dead
[Trait(TestCategories.Name, TestCategories.Unit)]
public class TransientErrorTests
{
    // CE-40
    [Fact]
    public void IsTransient_NpgsqlSocketError_ReturnsTrue()
    {
        var exception = new NpgsqlException("Connection refused", new SocketException((int)SocketError.ConnectionRefused));

        TenantEventsConsumer.IsTransient(exception).ShouldBeTrue();
    }

    // CE-41
    [Fact]
    public void IsTransient_TimeoutWrappedInDbUpdateException_ReturnsTrue()
    {
        var exception = new DbUpdateException("Save failed", new TimeoutException());

        TenantEventsConsumer.IsTransient(exception).ShouldBeTrue();
    }

    // CE-43
    [Fact]
    public void IsTransient_InvalidOperation_ReturnsFalse()
    {
        TenantEventsConsumer.IsTransient(new InvalidOperationException()).ShouldBeFalse();
    }
}
