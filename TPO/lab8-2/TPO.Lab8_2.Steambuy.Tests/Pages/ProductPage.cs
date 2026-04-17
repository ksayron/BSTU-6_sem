using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace TPO.Lab8_2.Tests.Pages;

public sealed class ProductPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By TitleLocator = By.CssSelector("h1");
    private static readonly By BuyNowButton = By.CssSelector("a[href*='buy'], a[class*='buy'], button[class*='buy']");

    public ProductPage WaitUntilLoaded()
    {
        Wait.Until(d => d.Url.Contains("/steam/", StringComparison.OrdinalIgnoreCase) ||
                        d.FindElements(TitleLocator).Any(e => e.Displayed));

        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Product page is blocked by anti-bot protection.");
        }

        return this;
    }

    public void OpenKnownProduct()
    {
        Driver.Navigate().GoToUrl("https://steambuy.com/steam/resident-evil-village/");
        WaitUntilLoaded();
    }

    public string GetTitle()
    {
        return WaitVisible(TitleLocator).Text.Trim();
    }

    public bool HasPriceLikeValue()
    {
        var text = Driver.PageSource;
        return Regex.IsMatch(text, @"\b\d{2,6}\b");
    }

    public bool HasKeyDetails()
    {
        var source = Driver.PageSource;
        return source.Contains("Steam", StringComparison.OrdinalIgnoreCase) ||
               source.Contains("delivery", StringComparison.OrdinalIgnoreCase) ||
               Driver.FindElements(BuyNowButton).Any(e => e.Displayed);
    }
}
