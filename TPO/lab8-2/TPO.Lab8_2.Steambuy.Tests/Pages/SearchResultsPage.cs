using OpenQA.Selenium;

namespace TPO.Lab8_2.Tests.Pages;

public sealed class SearchResultsPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By ResultLinks = By.CssSelector("a[href*='/steam/']");

    public SearchResultsPage WaitUntilLoaded()
    {
        Wait.Until(d => d.Url.Contains("/catalog", StringComparison.OrdinalIgnoreCase));
        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Search/catalog content is blocked by anti-bot protection.");
        }

        return this;
    }

    public int GetResultsCount()
    {
        return Driver.FindElements(ResultLinks).Count;
    }

    public IReadOnlyList<string> GetVisibleResultTitles()
    {
        return Driver.FindElements(ResultLinks)
            .Select(e => e.Text.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();
    }

    public bool HasRelevantResult(string query)
    {
        var titles = GetVisibleResultTitles();
        if (titles.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return Driver.PageSource.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    public ProductPage OpenFirstResult()
    {
        var first = WaitAny(ResultLinks).First();
        ScrollTo(first);
        first.Click();
        return new ProductPage(Driver).WaitUntilLoaded();
    }
}
