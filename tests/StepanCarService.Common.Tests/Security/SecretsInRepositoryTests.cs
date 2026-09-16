using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace StepanCarService.Common.Tests.Security;

// Секреты хранятся только в user-secrets и переменных окружения; в appsettings репозитория — пусто или заглушки your_*
[Trait(TestCategories.Name, TestCategories.Security)]
public partial class SecretsInRepositoryTests
{
    private static readonly string[] SecretKeyNames = ["key", "password", "secret", "token", "apikey", "clientsecret"];

    public static TheoryData<string> AppSettingsFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in RepositoryPaths.EnumerateSourceFiles("appsettings*.json", "Core", "Services"))
            data.Add(Path.GetRelativePath(RepositoryPaths.Root, file));
        return data;
    }

    [Fact]
    public void AppSettingsFiles_AreFound()
    {
        AppSettingsFiles().Count.ShouldBeGreaterThanOrEqualTo(8);
    }

    [Theory]
    [MemberData(nameof(AppSettingsFiles))]
    public void AppSettings_ContainNoSecrets(string relativePath)
    {
        var json = JsonNode.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.Root, relativePath)),
            documentOptions: new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, AllowTrailingCommas = true })!;

        FindSecrets(json, path: string.Empty).ShouldBeEmpty();
    }

    private static IEnumerable<string> FindSecrets(JsonNode? node, string path)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var (name, child) in obj)
                    foreach (var finding in FindSecrets(child, path.Length == 0 ? name : $"{path}:{name}"))
                        yield return finding;
                break;
            case JsonArray array:
                for (var i = 0; i < array.Count; i++)
                    foreach (var finding in FindSecrets(array[i], $"{path}:{i}"))
                        yield return finding;
                break;
            case JsonValue value when value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text):
                var leaf = path.Split(':')[^1].ToLowerInvariant();
                if (SecretKeyNames.Contains(leaf) && !IsPlaceholder(text))
                    yield return $"{path} = {text}";
                if (path.StartsWith("ConnectionStrings:", StringComparison.OrdinalIgnoreCase))
                {
                    var password = ConnectionStringPassword().Match(text);
                    if (password.Success && !IsPlaceholder(password.Groups["value"].Value))
                        yield return $"{path}: пароль в строке подключения";
                }
                break;
        }
    }

    private static bool IsPlaceholder(string value) => value.Length == 0 || value.StartsWith("your_", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"(?:^|;)\s*(?:password|pwd)\s*=\s*(?<value>[^;]*)", RegexOptions.IgnoreCase)]
    private static partial Regex ConnectionStringPassword();
}
