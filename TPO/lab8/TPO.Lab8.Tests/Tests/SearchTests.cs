using TPO.Lab8.Tests.Helpers;

namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class SearchTests : BaseTest
{
    [Test]
    [Order(1)]
    [Category("Smoke")]
    [Category("Search")]
    [TestCase("free")]
    [TestCase("rpg")]
    [TestCase("gui")]
    public void Search_ShouldShowRelevantResults(string term)
    {
        HomePage.Open();
        var searchResults = HomePage.Search(term);

        Assert.That(searchResults.GetResultCount(), Is.GreaterThan(0), "No search results were shown.");
        Assert.That(searchResults.HasResultRelatedTo(term), Is.True, $"No result looks related to '{term}'.");

        var screenshotPath = ScreenshotHelper.SaveScreenshot(Driver, $"search_{term}");
        ReportManager.AttachScreenshot(screenshotPath, $"Search results for '{term}'");
    }
}
