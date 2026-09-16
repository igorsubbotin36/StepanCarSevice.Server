using StepanCarSevice.AuthService.Infrastructure.Auth;

namespace StepanCarService.AuthService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    // AU-02
    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        _hasher.Hash("Secret-123").ShouldNotBe(_hasher.Hash("Secret-123"));
    }

    // AU-03
    [Theory]
    [InlineData("Secret-123", true)]
    [InlineData("secret-123", false)]
    public void Verify_ReturnsTrueOnlyForOriginalPassword(string candidate, bool expected)
    {
        var hash = _hasher.Hash("Secret-123");

        _hasher.Verify(candidate, hash).ShouldBe(expected);
    }
}
