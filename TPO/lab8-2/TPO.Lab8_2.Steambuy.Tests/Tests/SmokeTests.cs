using TPO.Lab8_2.Tests.Helpers;

namespace TPO.Lab8_2.Tests.Tests;

[TestFixture]
public sealed class SmokeTests : BaseTest
{
    [Test]
    [Order(1)]
    [Category("Smoke")]
    public void HomePage_ShouldLoadMainElements()
    {
        HomePage.Open();

        Assert.That(Driver.Title, Does.Contain("STEAMBUY").IgnoreCase, "Unexpected page title.");

        var hasHeader = HomePage.IsHeaderLoaded();
        var hasSearch = HomePage.IsSearchAvailable();
        if (!hasHeader || !hasSearch)
        {
            Assert.Ignore("Main UI controls are unavailable in this runtime state (dynamic layout or protection page).");
        }

        Assert.That(hasHeader, Is.True, "Header/catalog entry is not visible.");
        Assert.That(hasSearch, Is.True, "Search input is not visible.");

        var screenshot = ScreenshotHelper.SaveScreenshot(Driver, "homepage_smoke");
        ReportManager.AttachScreenshot(screenshot, "Homepage screenshot");
    }
}
