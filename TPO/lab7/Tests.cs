using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumLab.Task2.Pages;

namespace SeleniumLab.Task2;

// ══════════════════════════════════════════════════════════════════════════════
//  Task 2 — Automated Tests for demoqa.com
//
//  Covered:
//    ✓  Test case 1  — Text Box form: fill & verify output
//    ✓  Test case 2  — Checkbox tree: expand, check, verify selection
//    ✓  Dropdown / radio / checkbox test — Practice Form with all widget types
//    ✓  End-to-end scenario — Full student registration flow
//
//  Runs WITHOUT any test framework (xUnit/NUnit) so you can execute it as a
//  plain console app.  Every test prints PASS / FAIL and why.
// ══════════════════════════════════════════════════════════════════════════════

public class TestRunner
{
    private IWebDriver _driver = null!;
    private int _passed;
    private int _failed;

    public void RunAll()
    {
        Console.WriteLine("══════════════════════════════════════════════");
        Console.WriteLine("  TASK 2 — Automated Tests  (demoqa.com)");
        Console.WriteLine("══════════════════════════════════════════════\n");

        SetupDriver();

        try
        {
            TestCase1_TextBoxFillAndVerify();
            TestCase2_CheckboxTreeSelectionVerify();
            TestCase3_DropdownRadioCheckbox();
            E2E_StudentRegistrationFlow();
        }
        finally
        {
            _driver.Quit();
        }

        Console.WriteLine("\n══════════════════════════════════════════════");
        Console.WriteLine($"  Results: {_passed} PASSED  |  {_failed} FAILED");
        Console.WriteLine("══════════════════════════════════════════════");
    }

    // ──────────────────────────────────────────────────────────────────
    // Driver setup
    // ──────────────────────────────────────────────────────────────────

    private void SetupDriver()
    {
        var options = new EdgeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1280,900");

        _driver = new EdgeDriver(options);

        // IMPLICIT WAIT — applied globally to every FindElement call.
        // The driver will poll for up to 5 s before throwing.
        // Use EXPLICIT waits on top of this for specific conditions.
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    // ──────────────────────────────────────────────────────────────────
    // Test Case 1 — Text Box: fill form, verify output block
    // ──────────────────────────────────────────────────────────────────

    private void TestCase1_TextBoxFillAndVerify()
    {
        const string TestName = "TC1 — Text Box: fill and verify output";
        Console.WriteLine($"\n▶  {TestName}");

        try
        {
            var page = new TextBoxPage(_driver);
            page.Open();

            const string FullName  = "Mykola Kovalenko";
            const string Email     = "mykola@example.com";

            page.Fill(FullName, Email,
                      "123 Main Street, Kyiv",
                      "456 Second Avenue, Kyiv");
            page.ClickSubmit();

            // EXPLICIT WAIT — wait until the output section appears
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("output")).Displayed);

            // ── Assert name ──
            string actualName = page.OutputName;
            Assert(
                condition: actualName.Contains(FullName),
                message:   $"Expected name to contain '{FullName}', got '{actualName}'"
            );

            // ── Assert email ──
            string actualEmail = page.OutputEmail;
            Assert(
                condition: actualEmail.Contains(Email),
                message:   $"Expected email to contain '{Email}', got '{actualEmail}'"
            );

            Pass(TestName);
        }
        catch (Exception ex)
        {
            Fail(TestName, ex.Message);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Test Case 2 — Checkbox tree: expand, check "Downloads", verify
    // ──────────────────────────────────────────────────────────────────

    private void TestCase2_CheckboxTreeSelectionVerify()
    {
        const string TestName = "TC2 — Checkbox tree: expand all and select 'Downloads'";
        Console.WriteLine($"\n▶  {TestName}");

        try
        {
            var page = new CheckboxPage(_driver);
            page.Open();

            page.ExpandAll();

            // EXPLICIT WAIT — wait until the inner nodes are visible
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
            wait.Until(d =>
                d.FindElements(By.CssSelector(".rct-node .rct-title")).Count > 3);

            page.CheckItemByLabel("Downloads");

            // EXPLICIT WAIT — result panel must appear
            wait.Until(d => d.FindElement(By.Id("result")).Displayed);

            var selected = page.SelectedItems;

            // The "Downloads" node contains "wordFile" and "excelFile" as children.
            // demoqa shows child items when a parent is checked.
            Assert(
                condition: selected.Any(s => s.Contains("downloads", StringComparison.OrdinalIgnoreCase)
                                          || s.Contains("wordFile",  StringComparison.OrdinalIgnoreCase)),
                message:   $"Expected 'downloads' in selection results, got: [{string.Join(", ", selected)}]"
            );

            Pass(TestName);
        }
        catch (Exception ex)
        {
            Fail(TestName, ex.Message);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Test Case 3 — Dropdowns, Radio buttons, Checkboxes
    //               (Practice Form — partial submission with validation)
    // ──────────────────────────────────────────────────────────────────

    private void TestCase3_DropdownRadioCheckbox()
    {
        const string TestName = "TC3 — Practice Form: radio, checkbox, dropdown interactions";
        Console.WriteLine($"\n▶  {TestName}");

        try
        {
            var page = new PracticeFormPage(_driver);
            page.Open();

            // Fill mandatory text fields first
            page.EnterFirstName("Anna");
            page.EnterLastName("Shevchenko");
            page.EnterEmail("anna@example.com");

            // RADIO BUTTON — select "Female"
            page.SelectGenderFemale();

            page.EnterMobileNumber("9876543210");

            // CHECKBOX — check "Music" hobby
            page.CheckHobbyMusic();

            // DROPDOWN (react-select) — State then City
            page.SelectState("Haryana");
            page.SelectCity("Karnal");

            // EXPLICIT WAIT — city dropdown should reflect "Karnal"
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
            wait.Until(d =>
            {
                var cityText = d.FindElement(
                    By.CssSelector("#city .css-1uccc91-singleValue, #city [class$='singleValue']"));
                return cityText.Text.Contains("Karnal");
            });

            page.ClickSubmit();

            // Verify modal appeared
            wait.Until(d => d.FindElement(By.Id("example-modal-sizes-title-lg")).Displayed);

            var rows = page.GetConfirmationRows();

            // Assert gender
            AssertRowContains(rows, "Gender",    "Female");
            // Assert hobby
            AssertRowContains(rows, "Hobbies",   "Music");
            // Assert state/city
            AssertRowContains(rows, "State and City", "Haryana");

            Pass(TestName);
        }
        catch (Exception ex)
        {
            Fail(TestName, ex.Message);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // End-to-End Scenario — Full Student Registration
    // Simulates a complete user flow: open page → fill all fields →
    // submit → verify every field in the confirmation modal.
    // ──────────────────────────────────────────────────────────────────

    private void E2E_StudentRegistrationFlow()
    {
        const string TestName = "E2E — Full student registration flow";
        Console.WriteLine($"\n▶  {TestName}");

        try
        {
            var page = new PracticeFormPage(_driver);
            page.Open();

            // Step 1 — Personal data
            page.EnterFirstName("Ivan");
            page.EnterLastName("Petrenko");
            page.EnterEmail("ivan.petrenko@example.com");
            page.SelectGenderMale();
            page.EnterMobileNumber("0501234567");

            // Step 2 — Hobbies (multiple checkboxes)
            page.CheckHobbySports();
            page.CheckHobbyReading();

            // Step 3 — Location dropdowns
            page.SelectState("NCR");
            page.SelectCity("Delhi");

            // Step 4 — Submit
            page.ClickSubmit();

            // EXPLICIT WAIT — modal must be visible with title
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d =>
                d.FindElement(By.Id("example-modal-sizes-title-lg")).Displayed);

            // Step 5 — Verify ALL key fields
            var rows = page.GetConfirmationRows();

            AssertRowContains(rows, "Student Name", "Ivan Petrenko");
            AssertRowContains(rows, "Student Email", "ivan.petrenko@example.com");
            AssertRowContains(rows, "Gender",   "Male");
            AssertRowContains(rows, "Mobile",   "0501234567");
            AssertRowContains(rows, "Hobbies",  "Sports");
            AssertRowContains(rows, "Hobbies",  "Reading");
            AssertRowContains(rows, "State and City", "NCR");

            Pass(TestName);
        }
        catch (Exception ex)
        {
            Fail(TestName, ex.Message);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"Assertion failed: {message}");
    }

    private static void AssertRowContains(
        IReadOnlyList<(string Label, string Value)> rows,
        string labelContains,
        string valueContains)
    {
        var row = rows.FirstOrDefault(r =>
            r.Label.Contains(labelContains, StringComparison.OrdinalIgnoreCase));

        if (row.Label == null)
            throw new Exception($"No row with label containing '{labelContains}'. " +
                                 $"Rows: [{string.Join(" | ", rows.Select(r => r.Label))}]");

        if (!row.Value.Contains(valueContains, StringComparison.OrdinalIgnoreCase))
            throw new Exception($"Row '{row.Label}': expected value to contain '{valueContains}', " +
                                 $"got '{row.Value}'");
    }

    private void Pass(string name)
    {
        _passed++;
        Console.WriteLine($"  ✅  PASS — {name}");
    }

    private void Fail(string name, string reason)
    {
        _failed++;
        Console.WriteLine($"  ❌  FAIL — {name}");
        Console.WriteLine($"        Reason: {reason}");
    }
}
