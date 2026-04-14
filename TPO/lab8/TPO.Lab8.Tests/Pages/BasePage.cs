using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TPO.Lab8.Tests.Pages;

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
            var el = d.FindElement(locator);
            return el.Displayed ? el : null;
        })!;
    }

    protected bool TryFindVisible(By locator, out IWebElement? element)
    {
        try
        {
            element = Driver.FindElements(locator).FirstOrDefault(e => e.Displayed);
            return element != null;
        }
        catch
        {
            element = null;
            return false;
        }
    }

    protected IReadOnlyCollection<IWebElement> WaitAny(By locator)
    {
        return Wait.Until(d =>
        {
            var elements = d.FindElements(locator);
            return elements.Count > 0 ? elements : null;
        })!;
    }

    protected void SafeClick(By locator)
    {
        var element = WaitVisible(locator);
        ScrollTo(element);
        element.Click();
    }

    protected void ScrollTo(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
    }

    public void TryAcceptCookiesBanner()
    {
        var acceptButtonCandidates = new[]
        {
            By.XPath("//button[contains(translate(., 'ACEPTL', 'aceptl'),'accept')]"),
            By.XPath("//a[contains(translate(., 'ACEPTL', 'aceptl'),'accept')]")
        };

        foreach (var locator in acceptButtonCandidates)
        {
            var button = Driver.FindElements(locator).FirstOrDefault();
            if (button is { Displayed: true, Enabled: true })
            {
                button.Click();
                return;
            }
        }
    }

    protected bool IsLikelyBlockedByProtection()
    {
        var title = Driver.Title.ToLowerInvariant();
        var source = Driver.PageSource.ToLowerInvariant();

        return title.Contains("attention required") ||
               title.Contains("just a moment") ||
               source.Contains("cloudflare") ||
               source.Contains("captcha") ||
               source.Contains("access denied");
    }
}
