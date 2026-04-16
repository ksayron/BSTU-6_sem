using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace TPO.Lab8_2.Tests.Helpers;

public static class DriverFactory
{
    public static IWebDriver CreateDriver()
    {
        var browser = (Environment.GetEnvironmentVariable("SB_BROWSER") ?? "edge").Trim().ToLowerInvariant();
        var isHeadless = string.Equals(Environment.GetEnvironmentVariable("SB_HEADLESS"), "1", StringComparison.OrdinalIgnoreCase);

        return browser switch
        {
            "edge" => CreateEdgeDriver(isHeadless),
            _ => CreateChromeDriver(isHeadless)
        };
    }

    private static IWebDriver CreateChromeDriver(bool isHeadless)
    {
        var options = new ChromeOptions();
        ApplyCommonOptions(options, isHeadless);
        var chromeBinary = TryResolveChromeBinary();
        if (!string.IsNullOrWhiteSpace(chromeBinary))
        {
            options.BinaryLocation = chromeBinary;
        }
        options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);

        var service = ChromeDriverService.CreateDefaultService(AppContext.BaseDirectory);
        service.HideCommandPromptWindow = true;
        var driver = new ChromeDriver(service, options);
        ConfigureDriver(driver);
        return driver;
    }

    private static IWebDriver CreateEdgeDriver(bool isHeadless)
    {
        var options = new EdgeOptions();
        ApplyCommonOptions(options, isHeadless);
        var edgeBinary = TryResolveEdgeBinary();
        if (!string.IsNullOrWhiteSpace(edgeBinary))
        {
            options.BinaryLocation = edgeBinary;
        }

        var service = EdgeDriverService.CreateDefaultService(AppContext.BaseDirectory);
        service.HideCommandPromptWindow = true;
        var driver = new EdgeDriver(service, options);
        ConfigureDriver(driver);
        return driver;
    }

    private static void ApplyCommonOptions(ChromeOptions options, bool isHeadless)
    {
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");

        if (isHeadless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }
    }

    private static void ApplyCommonOptions(EdgeOptions options, bool isHeadless)
    {
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");

        if (isHeadless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }
    }

    private static void ConfigureDriver(IWebDriver driver)
    {
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(45);
    }

    private static string? TryResolveChromeBinary()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? TryResolveEdgeBinary()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
