using Bogus;

namespace StepanCarService.TestKit.Builders;

// Уникальные на тест значения: тесты не зависят от порядка выполнения и не конфликтуют по уникальным индексам
public static class TestData
{
    private static readonly Faker Faker = new("ru");
    private static long _counter = Random.Shared.Next(1_000_000);

    // Телефон в формате +7XXXXXXXXXX, уникальный в пределах процесса
    public static string Phone() => $"+79{Interlocked.Increment(ref _counter) % 1_000_000_000:D9}";

    // Допустимый идентификатор тенанта (DNS-метка в нижнем регистре)
    public static string TenantIdentifier(string prefix = "t") => $"{prefix}{Guid.NewGuid():N}"[..20];

    public static string FirstName() => Faker.Name.FirstName();
    public static string LastName() => Faker.Name.LastName();
    public static string Email() => $"user{Interlocked.Increment(ref _counter)}@example.com";
    public static string CompanyName() => $"Автосервис «{Faker.Company.CompanyName()}»";
    public static string Password() => $"Pwd-{Guid.NewGuid():N}"[..16];
}
