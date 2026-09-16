using System.Reflection;
using Xunit;

namespace StepanCarService.TestKit.Conventions;

// Guard-проверка структуры тестового проекта: тест не «потеряется» из фильтра --filter Category=...
//  - у каждого тестового класса ровно один трейт Category из TestCategories.All;
//  - значение совпадает с папкой (сегментом пространства имён): Unit/, Integration/, Api/ ...;
//    в проекте E2ETests категория E2E допускается и в корне.
public static class TestConventions
{
    public static IReadOnlyList<string> FindViolations(Assembly testAssembly)
    {
        var violations = new List<string>();
        var isE2EProject = testAssembly.GetName().Name?.EndsWith("E2ETests", StringComparison.Ordinal) == true;

        foreach (var type in testAssembly.GetTypes().Where(IsTestClass))
        {
            var categories = GetCategories(type).Distinct().ToList();
            if (categories.Count != 1)
            {
                violations.Add($"{type.FullName}: ожидается ровно один трейт {TestCategories.Name}, найдено: [{string.Join(", ", categories)}]");
                continue;
            }

            var category = categories[0];
            if (!TestCategories.All.Contains(category))
            {
                violations.Add($"{type.FullName}: неизвестная категория «{category}»");
                continue;
            }

            var segments = (type.Namespace ?? string.Empty).Split('.');
            var inMatchingFolder = segments.Contains(category) || (isE2EProject && category == TestCategories.E2E);
            var folderCategory = segments.FirstOrDefault(TestCategories.All.Contains);
            if (!inMatchingFolder || (folderCategory != null && folderCategory != category))
                violations.Add($"{type.FullName}: категория «{category}» не совпадает с папкой (пространство имён {type.Namespace})");
        }
        return violations;
    }

    private static bool IsTestClass(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Any(m => m.GetCustomAttributes<FactAttribute>(inherit: true).Any());

    private static IEnumerable<string> GetCategories(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            foreach (var attribute in current.GetCustomAttributesData())
            {
                if (attribute.AttributeType == typeof(TraitAttribute)
                    && attribute.ConstructorArguments.Count == 2
                    && attribute.ConstructorArguments[0].Value as string == TestCategories.Name
                    && attribute.ConstructorArguments[1].Value is string value)
                {
                    yield return value;
                }
            }
        }
    }
}
