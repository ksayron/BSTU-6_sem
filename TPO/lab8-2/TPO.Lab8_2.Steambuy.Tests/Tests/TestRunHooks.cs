using TPO.Lab8_2.Tests.Helpers;

namespace TPO.Lab8_2.Tests.Tests;

[SetUpFixture]
public sealed class TestRunHooks
{
    [OneTimeSetUp]
    public void BeforeAll()
    {
        ReportManager.EnsureInitialized();
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        ReportManager.Flush();
        TestContext.Out.WriteLine($"Extent report generated: {ReportManager.ReportPath}");
    }
}
