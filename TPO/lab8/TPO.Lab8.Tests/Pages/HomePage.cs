using OpenQA.Selenium;

namespace TPO.Lab8.Tests.Pages;

public sealed class HomePage(IWebDriver driver) : BasePage(driver)
{
    public const string Url = "https://www.gamedevmarket.net/";

    private static readonly By SearchInput = By.CssSelector(
        "input[placeholder*='looking for'], input[placeholder*='Looking for'], input[type='search'], input[name='s'], form[role='search'] input, input[id*='search']");

    public HomePage Open()
    {
        Driver.Navigate().GoToUrl(Url);
        TryAcceptCookiesBanner();

        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("GameDev Market returned anti-bot/captcha page in this environment.");
        }

        return this;
    }

    public SearchResultsPage Search(string term)
    {
        if (TryFindVisible(SearchInput, out var search))
        {
            search!.Clear();
            search.SendKeys(term);
            search.SendKeys(Keys.Enter);
        }
        else
        {
            Driver.Navigate().GoToUrl($"{Url}search-results?term={Uri.EscapeDataString(term)}");
        }

        return new SearchResultsPage(Driver).WaitUntilLoaded();
    }

    public LoginPage OpenLoginPage()
    {
        var loginLink = Driver.FindElements(By.XPath("//a[contains(.,'Login')]")).FirstOrDefault();
        if (loginLink == null)
        {
            throw new NoSuchElementException("Login link is not available on the home page.");
        }

        loginLink.Click();
        return new LoginPage(Driver).WaitUntilLoaded();
    }
}
