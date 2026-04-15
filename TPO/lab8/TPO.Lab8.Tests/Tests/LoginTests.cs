using TPO.Lab8.Tests.Helpers;

namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class LoginTests : BaseTest
{
    [Test]
    [Order(10)]
    [Category("Auth")]
    [Category("RequiresCredentials")]
    [Explicit("Run intentionally: requires a valid GameDev Market account and credentials in env variables.")]
    public void Login_WithEnvironmentCredentials_ShouldAuthenticate()
    {
        var email = Environment.GetEnvironmentVariable("GDM_EMAIL");
        var password = Environment.GetEnvironmentVariable("GDM_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Assert.Ignore("GDM_EMAIL/GDM_PASSWORD are missing. Login test skipped by design.");
        }

        LoginPage.Open();
        LoginPage.Login(email!, password!);

        var likelyAuthenticated = !Driver.Url.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                                  LoginPage.IsAuthenticatedIndicatorVisible() ||
                                  !LoginPage.IsLoginFormStillVisible();

        Assert.That(likelyAuthenticated, Is.True, "Login did not appear to succeed.");

        var screenshotPath = ScreenshotHelper.SaveScreenshot(Driver, "login_state");
        ReportManager.AttachScreenshot(screenshotPath, "State after login submit");
    }
}
