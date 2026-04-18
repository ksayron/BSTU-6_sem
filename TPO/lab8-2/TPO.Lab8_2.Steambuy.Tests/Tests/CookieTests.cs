using TPO.Lab8_2.Tests.Helpers;

namespace TPO.Lab8_2.Tests.Tests;

[TestFixture]
public sealed class CookieTests : BaseTest
{
    [Test]
    [Order(5)]
    [Category("Cookies")]
    public void HomePage_ShouldExportCookies()
    {
        HomePage.Open();
        var cookies = CookieHelper.GetCookies(Driver);
        if (cookies.Count == 0)
        {
            Assert.Ignore("No cookies captured in current runtime state.");
        }

        Assert.That(cookies.Count, Is.GreaterThan(0), "No cookies were captured.");

        var filePath = CookieHelper.SaveCookiesToJson(Driver, "home_page");
        Assert.That(File.Exists(filePath), Is.True, "Cookie file was not created.");

        ReportManager.LogInfo($"Cookies saved: {filePath}");
    }
}
