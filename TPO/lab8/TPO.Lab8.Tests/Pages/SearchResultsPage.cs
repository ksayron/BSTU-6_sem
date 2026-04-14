using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TPO.Lab8.Tests.Pages;

public sealed class SearchResultsPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By ResultCards = By.CssSelector("a[href*='/asset/']");
    private static readonly By OrderBySelect = By.XPath(
        "//label[contains(translate(., 'ORDERBY', 'orderby'),'order by')]/following::select[1]");

    public SearchResultsPage WaitUntilLoaded()
    {
        Wait.Until(d =>
        {
            var isSearch = d.Url.Contains("/search-results", StringComparison.OrdinalIgnoreCase);
            var hasResults = d.FindElements(ResultCards).Count > 0;
            return isSearch || hasResults;
        });

        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Search results are blocked by anti-bot/captcha in this environment.");
        }

        return this;
    }

    public int GetResultCount()
    {
        return Driver.FindElements(ResultCards).Count;
    }

    public IReadOnlyList<string> GetResultTitles()
    {
        return Driver.FindElements(ResultCards)
            .Select(e => e.Text.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }

    public bool HasResultRelatedTo(string expectedText)
    {
        var titles = GetResultTitles();
        var foundInTitles = titles.Any(t => t.Contains(expectedText, StringComparison.OrdinalIgnoreCase));
        var foundInPageText = Driver.PageSource.Contains(expectedText, StringComparison.OrdinalIgnoreCase);
        return foundInTitles || foundInPageText;
    }

    public ProductPage OpenFirstProduct()
    {
        var first = WaitAny(ResultCards).First();
        ScrollTo(first);
        first.Click();
        return new ProductPage(Driver).WaitUntilLoaded();
    }

    public int GetOrderByOptionsCount()
    {
        var selectElement = new SelectElement(WaitVisible(OrderBySelect));
        return selectElement.Options.Count;
    }

    public string GetSelectedOrderByText()
    {
        var selectElement = new SelectElement(WaitVisible(OrderBySelect));
        return selectElement.SelectedOption.Text.Trim();
    }

    public void SelectOrderByIndex(int index)
    {
        var selectElement = new SelectElement(WaitVisible(OrderBySelect));
        if (selectElement.Options.Count <= index)
        {
            throw new InvalidOperationException($"Not enough sort options. Requested index={index}, actual={selectElement.Options.Count}.");
        }

        selectElement.SelectByIndex(index);
        Wait.Until(_ => !string.IsNullOrWhiteSpace(selectElement.SelectedOption.Text));
    }
}
