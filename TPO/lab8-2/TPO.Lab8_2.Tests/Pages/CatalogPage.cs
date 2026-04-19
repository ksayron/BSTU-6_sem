using OpenQA.Selenium;

namespace TPO.Lab8_2.Tests.Pages;

public sealed class CatalogPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By ResultLinks = By.CssSelector("a[href*='/steam/']");

    public CatalogPage WaitUntilLoaded()
    {
        Wait.Until(d => d.Url.Contains("/catalog", StringComparison.OrdinalIgnoreCase));
        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Catalog page is blocked by anti-bot protection.");
        }

        return this;
    }

    public int GetResultCount()
    {
        return Driver.FindElements(ResultLinks).Count;
    }

    public string CurrentUrl => Driver.Url;

    public bool HasSortControls()
    {
        return Driver.PageSource.Contains("sort", StringComparison.OrdinalIgnoreCase) ||
               Driver.PageSource.Contains("Sort", StringComparison.OrdinalIgnoreCase) ||
               Driver.FindElements(By.CssSelector("select[name*='sort'], [data-sort], .sort")).Count > 0;
    }

    public void ApplySortByPriceAsc()
    {
        Driver.Navigate().GoToUrl("https://steambuy.com/catalog/?sort=price_asc");
        WaitUntilLoaded();
    }
}
