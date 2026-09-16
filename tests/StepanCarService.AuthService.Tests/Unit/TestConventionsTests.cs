using StepanCarService.TestKit.Conventions;

namespace StepanCarService.AuthService.Tests.Unit;

[Trait(TestCategories.Name, TestCategories.Unit)]
public class TestConventionsTests
{
    [Fact]
    public void TestClasses_HaveOneCategoryMatchingFolder()
    {
        TestConventions.FindViolations(typeof(TestConventionsTests).Assembly).ShouldBeEmpty();
    }
}
