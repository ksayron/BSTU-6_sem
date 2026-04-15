using TPO.Lab8.Tests.Helpers;

namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class CookieTests : BaseTest
{
    [Test]
    [Order(4)]
    [Category("Cookies")]
    public void HomePage_ShouldExportCookiesToArtifacts()
    {
        HomePage.Open();
        var cookies = CookieHelper.GetCookies(Driver);
        Assert.That(cookies.Count, Is.GreaterThan(0), "No cookies were captured on the target page.");

        var filePath = CookieHelper.SaveCookiesToJson(Driver, "home_page");
        Assert.That(File.Exists(filePath), Is.True, "Cookie file was not created.");

        ReportManager.LogInfo($"Cookies exported to: {filePath}");
    }
}
