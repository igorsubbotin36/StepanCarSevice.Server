namespace StepanCarService.TestKit;

// Значения трейта Category: [Trait(TestCategories.Name, TestCategories.Unit)].
// Фильтр прогона: dotnet test --filter Category=Unit
public static class TestCategories
{
    public const string Name = "Category";

    // Без БД и брокера, выполняются параллельно
    public const string Unit = "Unit";
    // PostgreSQL в контейнере
    public const string Integration = "Integration";
    // RabbitMQ (и при необходимости PostgreSQL) в контейнерах
    public const string Messaging = "Messaging";
    // Сервис в WebApplicationFactory
    public const string Api = "Api";
    // Несколько сервисов вместе
    public const string E2E = "E2E";
    public const string Security = "Security";
    public const string Startup = "Startup";
    public const string Migrations = "Migrations";
    public const string Performance = "Performance";

    public static readonly IReadOnlyList<string> All =
        [Unit, Integration, Messaging, Api, E2E, Security, Startup, Migrations, Performance];
}

// Коллекции xUnit: тесты одной коллекции выполняются последовательно, разные коллекции — параллельно
public static class TestCollections
{
    public const string Database = "Database";
    public const string RabbitMq = "RabbitMq";
}
