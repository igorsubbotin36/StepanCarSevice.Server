using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace StepanCarService.TestKit.Messaging;

// Один брокер RabbitMQ на процесс тестового проекта, запускается при первом обращении.
// Изоляция тестовых классов — через отдельный virtual host (RabbitMqVirtualHost)
public static class RabbitMqContainer
{
    public const string Image = "rabbitmq:3-management";
    public const string UserName = "test";
    public const string Password = "test";
    public const int AmqpPort = 5672;
    public const int ManagementPort = 15672;

    private static readonly Lazy<Task<Testcontainers.RabbitMq.RabbitMqContainer>> Container =
        new(StartAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public static async Task<Testcontainers.RabbitMq.RabbitMqContainer> GetAsync() => await Container.Value;

    public static async Task<string> CreateVirtualHostAsync()
    {
        var container = await GetAsync();
        var name = $"test-{Guid.NewGuid():N}";
        await ExecAsync(container, "rabbitmqctl", "add_vhost", name);
        await ExecAsync(container, "rabbitmqctl", "set_permissions", "-p", name, UserName, ".*", ".*", ".*");
        return name;
    }

    public static async Task DeleteVirtualHostAsync(string name)
    {
        var container = await GetAsync();
        await ExecAsync(container, "rabbitmqctl", "delete_vhost", name);
    }

    public static async Task<ConnectionFactory> CreateConnectionFactoryAsync(string virtualHost)
    {
        var container = await GetAsync();
        return new ConnectionFactory
        {
            HostName = container.Hostname,
            Port = container.GetMappedPublicPort(AmqpPort),
            UserName = UserName,
            Password = Password,
            VirtualHost = virtualHost
        };
    }

    private static async Task ExecAsync(Testcontainers.RabbitMq.RabbitMqContainer container, params string[] command)
    {
        var result = await container.ExecAsync(command);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"{string.Join(' ', command)} завершилась с кодом {result.ExitCode}: {result.Stderr}");
    }

    private static async Task<Testcontainers.RabbitMq.RabbitMqContainer> StartAsync()
    {
        var container = new RabbitMqBuilder(Image)
            .WithUsername(UserName)
            .WithPassword(Password)
            .WithPortBinding(ManagementPort, true)
            .Build();
        await container.StartAsync();
        return container;
    }
}
