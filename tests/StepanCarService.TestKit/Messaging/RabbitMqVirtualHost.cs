using RabbitMQ.Client;

namespace StepanCarService.TestKit.Messaging;

// Фикстура «свой virtual host на тестовый класс»: IClassFixture<RabbitMqVirtualHost>.
// Очереди и exchange одного класса не видны другим; после класса vhost удаляется вместе со всем содержимым
public sealed class RabbitMqVirtualHost : IAsyncLifetime
{
    public string Name { get; private set; } = string.Empty;
    public string HostName { get; private set; } = string.Empty;
    public int Port { get; private set; }
    public string UserName => RabbitMqContainer.UserName;
    public string Password => RabbitMqContainer.Password;

    public async ValueTask InitializeAsync()
    {
        Name = await RabbitMqContainer.CreateVirtualHostAsync();
        var factory = await RabbitMqContainer.CreateConnectionFactoryAsync(Name);
        HostName = factory.HostName;
        Port = factory.Port;
    }

    public Task<ConnectionFactory> CreateConnectionFactoryAsync() => RabbitMqContainer.CreateConnectionFactoryAsync(Name);

    // Настройки секции "RabbitMQ" сервиса, указывающие на этот vhost
    public IReadOnlyDictionary<string, string> ToSettings(string exchangeName, string? queueName = null)
    {
        var settings = new Dictionary<string, string>
        {
            ["RabbitMQ:HostName"] = HostName,
            ["RabbitMQ:Port"] = Port.ToString(),
            ["RabbitMQ:UserName"] = UserName,
            ["RabbitMQ:Password"] = Password,
            ["RabbitMQ:VirtualHost"] = Name,
            ["RabbitMQ:ExchangeName"] = exchangeName
        };
        if (queueName != null)
            settings["RabbitMQ:QueueName"] = queueName;
        return settings;
    }

    public async ValueTask DisposeAsync()
    {
        if (!string.IsNullOrEmpty(Name))
            await RabbitMqContainer.DeleteVirtualHostAsync(Name);
    }
}
