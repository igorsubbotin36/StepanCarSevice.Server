using StepanCarSevice.AuthService.Infrastructure.Auth;

namespace StepanCarService.AuthService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_HasIterationsSaltAndKeyFormat()
    {
        var parts = _hasher.Hash("Secret-123").Split(':');

        parts.Length.ShouldBe(3);
        parts[0].ShouldBe("100000");
        Convert.FromBase64String(parts[1]).Length.ShouldBe(16);
        Convert.FromBase64String(parts[2]).Length.ShouldBe(32);
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        _hasher.Hash("Secret-123").ShouldNotBe(_hasher.Hash("Secret-123"));
    }

    [Theory]
    [InlineData("Secret-123", true)]
    [InlineData("secret-123", false)]
    public void Verify_ReturnsTrueOnlyForOriginalPassword(string candidate, bool expected)
    {
        var hash = _hasher.Hash("Secret-123");

        _hasher.Verify(candidate, hash).ShouldBe(expected);
    }

    // Повреждённый хэш в БД — «пароль не подходит», а не 500
    [Theory]
    [InlineData("garbage")]
    [InlineData("100000:AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("many:AAAAAAAAAAAAAAAAAAAAAA==:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("100000:not base64!:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("100000:AAAAAAAAAAAAAAAAAAAAAA==:not base64!")]
    [InlineData("100000:AAAA:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("100000:AAAAAAAAAAAAAAAAAAAAAA==:AAAA")]
    [InlineData("100000:AAAAAAAAAAAAAAAAAAAAAA==:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=:extra")]
    public void Verify_CorruptedHash_ReturnsFalse(string hash)
    {
        Should.NotThrow(() => _hasher.Verify("Secret-123", hash)).ShouldBeFalse();
    }

    // Подменённый хэш не должен заставить сервер считать миллиарды итераций
    [Theory]
    [InlineData(9_999)]
    [InlineData(1_000_001)]
    [InlineData(int.MaxValue)]
    public void Verify_IterationsOutOfRange_ReturnsFalse(int iterations)
    {
        var parts = _hasher.Hash("Secret-123").Split(':');

        _hasher.Verify("Secret-123", $"{iterations}:{parts[1]}:{parts[2]}").ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Verify_EmptyPasswordOrHash_ReturnsFalse(string? empty)
    {
        _hasher.Verify(empty!, _hasher.Hash("Secret-123")).ShouldBeFalse();
        _hasher.Verify("Secret-123", empty!).ShouldBeFalse();
    }

    [Theory]
    [InlineData("Пароль-кириллицей")]
    [InlineData("пароль🔑с эмодзи")]
    public void HashAndVerify_UnicodePassword(string password)
    {
        var hash = _hasher.Hash(password);

        _hasher.Verify(password, hash).ShouldBeTrue();
        _hasher.Verify(password.ToUpperInvariant(), hash).ShouldBeFalse();
    }
}
