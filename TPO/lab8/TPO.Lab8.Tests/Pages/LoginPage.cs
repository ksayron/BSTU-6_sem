using OpenQA.Selenium;

namespace TPO.Lab8.Tests.Pages;

public sealed class LoginPage(IWebDriver driver) : BasePage(driver)
{
    public const string Url = "https://www.gamedevmarket.net/login";

    private static readonly By EmailOrUsername = By.CssSelector(
        "input[placeholder*='Email or username'], input[name='email'], input[name='username'], input[type='email']");

    private static readonly By Password = By.CssSelector(
        "input[placeholder='Password'], input[name='password'], input[type='password']");

    private static readonly By SubmitButton = By.CssSelector(
        "button[type='submit'], input[type='submit'], button[name='login']");

    public LoginPage Open()
    {
        Driver.Navigate().GoToUrl(Url);
        return WaitUntilLoaded();
    }

    public LoginPage WaitUntilLoaded()
    {
        WaitVisible(EmailOrUsername);
        WaitVisible(Password);
        return this;
    }

    public void Login(string emailOrUsername, string password)
    {
        var emailInput = WaitVisible(EmailOrUsername);
        emailInput.Clear();
        emailInput.SendKeys(emailOrUsername);

        var passwordInput = WaitVisible(Password);
        passwordInput.Clear();
        passwordInput.SendKeys(password);

        SafeClick(SubmitButton);
    }

    public bool IsLoginFormStillVisible()
    {
        return Driver.FindElements(EmailOrUsername).Any(e => e.Displayed);
    }

    public bool IsAuthenticatedIndicatorVisible()
    {
        var possibleIndicators = new[]
        {
            By.XPath("//a[contains(.,'Dashboard')]"),
            By.XPath("//a[contains(.,'Logout')]"),
            By.XPath("//a[contains(.,'My Account')]")
        };

        return possibleIndicators.Any(locator => Driver.FindElements(locator).Any(e => e.Displayed));
    }
}
