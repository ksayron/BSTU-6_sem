using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace TPO.Lab8.Tests.Helpers;

public static class ReportManager
{
    private static readonly object SyncRoot = new();
    private static readonly AsyncLocal<ExtentTest?> CurrentTest = new();
    private static ExtentReports? _extent;
    private static string? _reportPath;

    public static string ReportPath
    {
        get
        {
            EnsureInitialized();
            return _reportPath!;
        }
    }

    public static void EnsureInitialized()
    {
        if (_extent != null)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_extent != null)
            {
                return;
            }

            var fileName = $"ExtentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            _reportPath = Path.Combine(PathHelper.ReportsRoot, fileName);

            var spark = new ExtentSparkReporter(_reportPath);
            spark.Config.DocumentTitle = "Lab 8 Selenium Report";
            spark.Config.ReportName = "GameDev Market UI tests";

            _extent = new ExtentReports();
            _extent.AttachReporter(spark);
            _extent.AddSystemInfo("Environment", "Lab 8");
            _extent.AddSystemInfo("Target", "https://www.gamedevmarket.net/");
        }
    }

    public static void StartTest(string testName, string? description = null)
    {
        EnsureInitialized();
        var test = _extent!.CreateTest(testName, description);
        CurrentTest.Value = test;
    }

    public static void LogInfo(string message)
    {
        CurrentTest.Value?.Info(message);
    }

    public static void LogPass(string message)
    {
        CurrentTest.Value?.Pass(message);
    }

    public static void LogFail(string message)
    {
        CurrentTest.Value?.Fail(message);
    }

    public static void LogSkip(string message)
    {
        CurrentTest.Value?.Skip(message);
    }

    public static void AttachScreenshot(string screenshotPath, string title = "Screenshot")
    {
        var media = MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build();
        CurrentTest.Value?.Info(title, media);
    }

    public static void Flush()
    {
        _extent?.Flush();
    }
}
