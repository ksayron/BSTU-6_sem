using System.Text.Json;
using OpenQA.Selenium;

namespace TPO.Lab8_2.Tests.Helpers;

public static class CookieHelper
{
    public static string SaveCookiesToJson(IWebDriver driver, string scenarioName)
    {
        var cookies = driver.Manage().Cookies.AllCookies
            .Select(c => new SerializableCookie(
                c.Name,
                c.Value,
                c.Domain,
                c.Path,
                c.Expiry,
                c.Secure,
                c.IsHttpOnly,
                c.SameSite.ToString()))
            .ToList();

        var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{SanitizeName(scenarioName)}_cookies.json";
        var fullPath = Path.Combine(PathHelper.CookiesRoot, fileName);
        var json = JsonSerializer.Serialize(cookies, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fullPath, json);

        TestContext.Out.WriteLine($"Saved {cookies.Count} cookies to: {fullPath}");
        return fullPath;
    }

    public static IReadOnlyCollection<Cookie> GetCookies(IWebDriver driver)
    {
        return driver.Manage().Cookies.AllCookies;
    }

    private static string SanitizeName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Replace(' ', '_');
    }

    private sealed record SerializableCookie(
        string Name,
        string Value,
        string Domain,
        string Path,
        DateTime? Expiry,
        bool Secure,
        bool HttpOnly,
        string SameSite);
}
