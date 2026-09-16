namespace StepanCarService.TestKit;

// Пути внутри репозитория для тестов, проверяющих файлы исходников и конфигурации
public static class RepositoryPaths
{
    private const string SolutionFile = "StepanCarSevice.Server.sln";

    private static readonly Lazy<string> RootPath = new(() =>
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
                return directory.FullName;
        }
        throw new InvalidOperationException($"Не найден корень репозитория ({SolutionFile}) выше {AppContext.BaseDirectory}");
    });

    public static string Root => RootPath.Value;

    // Файлы исходников и конфигурации без bin/obj
    public static IEnumerable<string> EnumerateSourceFiles(string searchPattern, params string[] topDirectories) =>
        topDirectories
            .SelectMany(top => Directory.EnumerateFiles(Path.Combine(Root, top), searchPattern, SearchOption.AllDirectories))
            .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part => part is "bin" or "obj"));
}
