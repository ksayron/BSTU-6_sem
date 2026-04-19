using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TPO.Lab8_2.Tests.Pages;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver, int waitSeconds = 15)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
    }

    protected IWebElement WaitVisible(By locator)
    {
        return Wait.Until(d =>
        {
            var element = d.FindElement(locator);
            return element.Displayed ? element : null;
        })!;
    }

    protected IReadOnlyCollection<IWebElement> WaitAny(By locator)
    {
        return Wait.Until(d =>
        {
            var elements = d.FindElements(locator);
            return elements.Count > 0 ? elements : null;
        })!;
    }

    protected bool TryFindVisible(By locator, out IWebElement? element)
    {
        element = Driver.FindElements(locator).FirstOrDefault(e => e.Displayed);
        return element != null;
    }

    protected void ScrollTo(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
    }

    protected void SafeClick(By locator)
    {
        var element = WaitVisible(locator);
        ScrollTo(element);
        element.Click();
    }

    protected bool IsLikelyBlockedByProtection()
    {
        var title = Driver.Title.ToLowerInvariant();
        var source = Driver.PageSource.ToLowerInvariant();

        return title.Contains("just a moment") ||
               title.Contains("attention required") ||
               source.Contains("cloudflare") ||
               source.Contains("captcha") ||
               source.Contains("access denied");
    }

    public void TryAcceptCookiesBanner()
    {
        var candidates = new[]
        {
            By.XPath("//button[contains(.,'Accept')]"),
            By.XPath("//button[contains(.,'OK')]"),
            By.XPath("//a[contains(.,'Accept')]")
        };

        foreach (var locator in candidates)
        {
            var button = Driver.FindElements(locator).FirstOrDefault();
            if (button is { Displayed: true, Enabled: true })
            {
                button.Click();
                return;
            }
        }
    }
}
