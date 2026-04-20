using OpenQA.Selenium;

namespace TPO.Lab8_2.Tests.Pages;

public sealed class HomePage(IWebDriver driver) : BasePage(driver)
{
    public const string Url = "https://steambuy.com/";

    private static readonly By SearchInput = By.CssSelector(
        "input[name='q'], input[type='search'], input[type='text'][name*='search'], .search input");

    private static readonly By CatalogLink = By.CssSelector("a[href*='/catalog']");

    public HomePage Open()
    {
        Driver.Navigate().GoToUrl(Url);
        TryAcceptCookiesBanner();

        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Site returned anti-bot/captcha page.");
        }

        Wait.Until(d => d.Title.Contains("STEAMBUY", StringComparison.OrdinalIgnoreCase));
        return this;
    }

    public bool IsHeaderLoaded()
    {
        return Driver.FindElements(CatalogLink).Count > 0;
    }

    public bool IsSearchAvailable()
    {
        return Driver.FindElements(SearchInput).Any(e => e.Displayed);
    }

    public SearchResultsPage Search(string query)
    {
        if (TryFindVisible(SearchInput, out var input))
        {
            input!.Clear();
            input.SendKeys(query);
            input.SendKeys(Keys.Enter);
        }
        else
        {
            Driver.Navigate().GoToUrl($"https://steambuy.com/catalog/?q={Uri.EscapeDataString(query)}");
        }

        return new SearchResultsPage(Driver).WaitUntilLoaded();
    }

    public CatalogPage OpenCatalog()
    {
        var catalog = Driver.FindElements(CatalogLink).FirstOrDefault(e => e.Displayed);
        if (catalog != null)
        {
            catalog.Click();
        }
        else
        {
            Driver.Navigate().GoToUrl("https://steambuy.com/catalog/");
        }

        return new CatalogPage(Driver).WaitUntilLoaded();
    }
}
