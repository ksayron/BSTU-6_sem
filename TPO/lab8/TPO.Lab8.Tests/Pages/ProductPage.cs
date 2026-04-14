using OpenQA.Selenium;

namespace TPO.Lab8.Tests.Pages;

public sealed class ProductPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By TitleLocator = By.CssSelector("h1");
    private static readonly By CategoryLinks = By.CssSelector("a[href*='/category/']");

    public ProductPage WaitUntilLoaded()
    {
        Wait.Until(d =>
            d.Url.Contains("/asset/", StringComparison.OrdinalIgnoreCase) ||
            d.FindElements(TitleLocator).Any(e => e.Displayed));

        if (IsLikelyBlockedByProtection())
        {
            Assert.Ignore("Product page is blocked by anti-bot/captcha in this environment.");
        }

        return this;
    }

    public string GetTitle()
    {
        return WaitVisible(TitleLocator).Text.Trim();
    }

    public bool HasPriceOrFreeIndicator()
    {
        var pageText = Driver.PageSource;
        return pageText.Contains("$", StringComparison.OrdinalIgnoreCase) ||
               pageText.Contains("FREE!", StringComparison.OrdinalIgnoreCase) ||
               pageText.Contains("Free", StringComparison.OrdinalIgnoreCase);
    }

    public bool HasCategoryInformation()
    {
        return Driver.FindElements(CategoryLinks).Any();
    }
}
