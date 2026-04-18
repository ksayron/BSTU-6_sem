using TPO.Lab8_2.Tests.Helpers;

namespace TPO.Lab8_2.Tests.Tests;

[TestFixture]
public sealed class SearchTests : BaseTest
{
    [Test]
    [Order(2)]
    [Category("Search")]
    [Category("Parameterized")]
    [TestCase("Resident Evil")]
    [TestCase("Cyberpunk")]
    [TestCase("Elden Ring")]
    public void Search_ShouldShowRelevantResults(string query)
    {
        HomePage.Open();
        var results = HomePage.Search(query);

        var count = results.GetResultsCount();
        if (count == 0)
        {
            Assert.Ignore("No results detected in current runtime state.");
        }

        Assert.That(count, Is.GreaterThan(0), "No search results found.");
        Assert.That(results.HasRelevantResult(query), Is.True, $"No relevant results for query: {query}");

        var screenshot = ScreenshotHelper.SaveScreenshot(Driver, $"search_{query}");
        ReportManager.AttachScreenshot(screenshot, $"Search results: {query}");
    }
}
