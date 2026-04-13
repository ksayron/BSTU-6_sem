using OpenQA.Selenium;

namespace TPO.Lab8.Tests.Helpers;

public static class ScreenshotHelper
{
    public static string SaveScreenshot(IWebDriver driver, string scenarioName)
    {
        if (driver is not ITakesScreenshot screenshotDriver)
        {
            throw new InvalidOperationException("Driver does not support screenshots.");
        }

        var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{SanitizeName(scenarioName)}.png";
        var fullPath = Path.Combine(PathHelper.ScreenshotsRoot, fileName);
        screenshotDriver.GetScreenshot().SaveAsFile(fullPath);
        return fullPath;
    }

    private static string SanitizeName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Replace(' ', '_');
    }
}
