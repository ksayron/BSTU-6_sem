namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class TestManagementTests : BaseTest
{
    [Test]
    [Order(100)]
    [Category("ManagementDemo")]
    [Ignore("Demonstration of ignored/skip handling for lab 8 requirement.")]
    public void Ignored_DemoTest()
    {
        Assert.Fail("This test should never run because it is ignored.");
    }
}
