using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using TPO.Lab8.Tests.Helpers;
using TPO.Lab8.Tests.Pages;

namespace TPO.Lab8.Tests.Tests;

public abstract class BaseTest
{
    private IWebDriver? _driver;
    private HomePage? _homePage;
    private LoginPage? _loginPage;

    protected IWebDriver Driver => _driver ?? throw new InvalidOperationException("Driver is not initialized.");
    protected HomePage HomePage => _homePage ?? throw new InvalidOperationException("HomePage is not initialized.");
    protected LoginPage LoginPage => _loginPage ?? throw new InvalidOperationException("LoginPage is not initialized.");

    [SetUp]
    public void SetUp()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var testParams = TestContext.CurrentContext.Test.Arguments.Length > 0
            ? $"Parameters: {string.Join(", ", TestContext.CurrentContext.Test.Arguments.Select(a => a?.ToString() ?? "<null>"))}"
            : "No parameters";

        ReportManager.StartTest(testName, testParams);
        _driver = DriverFactory.CreateDriver();
        _homePage = new HomePage(_driver);
        _loginPage = new LoginPage(_driver);
        ReportManager.LogInfo("WebDriver started.");
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var details = TestContext.CurrentContext.Result.Message;

            if (status == TestStatus.Failed)
            {
                if (_driver != null)
                {
                    var failShot = ScreenshotHelper.SaveScreenshot(_driver, $"FAILED_{TestContext.CurrentContext.Test.Name}");
                    ReportManager.AttachScreenshot(failShot, "Failure screenshot");
                }

                ReportManager.LogFail($"Failed: {details}");
            }
            else if (status == TestStatus.Passed)
            {
                ReportManager.LogPass("Passed.");
            }
            else if (status == TestStatus.Skipped)
            {
                ReportManager.LogSkip($"Skipped: {details}");
            }
        }
        finally
        {
            _driver?.Quit();
            ReportManager.LogInfo("WebDriver closed.");
        }
    }
}
