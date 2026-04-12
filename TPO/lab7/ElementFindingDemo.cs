using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace SeleniumLab.Task1;

/// <summary>
/// Task 1 — Element finding on demoqa.com using all required locator strategies.
/// Site: https://demoqa.com/text-box  (Text Box page — rich with stable ids)
///       https://demoqa.com/links      (for link text locator)
///       https://demoqa.com/elements   (for list of elements)
/// </summary>
public class ElementFindingDemo
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    public void Run()
    {
        SetupDriver();

        Console.WriteLine("=== TASK 1: Element Finding Demo ===\n");

        FindById();
        FindByName();
        FindByCssSelectors();
        FindByXPaths();
        FindByPartialLinkText();
        FindMultipleElements();

        Console.WriteLine("\n=== All locator strategies completed successfully ===");
        _driver.Quit();
    }

    // ──────────────────────────────────────────────────────────────────
    // Setup
    // ──────────────────────────────────────────────────────────────────

    private void SetupDriver()
    {
        var options = new ChromeOptions();
        // Remove "--headless" if you want to watch the browser
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1280,900");

        _driver = new ChromeDriver(options);

        // Implicit wait — applied globally; the driver waits up to 5s
        // before throwing NoSuchElementException on every FindElement call.
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        // Explicit wait helper — used when we need a specific condition.
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    // ──────────────────────────────────────────────────────────────────
    // 1. By.Id
    // ──────────────────────────────────────────────────────────────────

    private void FindById()
    {
        Console.WriteLine("--- By.Id ---");
        _driver.Navigate().GoToUrl("https://demoqa.com/text-box");

        // Wait until the page heading is visible (explicit wait)
        _wait.Until(d => d.FindElement(By.CssSelector("h1.text-center")).Displayed);

        var fullNameInput = _driver.FindElement(By.Id("userName"));
        var emailInput    = _driver.FindElement(By.Id("userEmail"));

        Console.WriteLine($"  [FOUND] #userName  — tag: <{fullNameInput.TagName}>, placeholder: \"{fullNameInput.GetAttribute("placeholder")}\"");
        Console.WriteLine($"  [FOUND] #userEmail — tag: <{emailInput.TagName}>, placeholder: \"{emailInput.GetAttribute("placeholder")}\"");
    }

    // ──────────────────────────────────────────────────────────────────
    // 2. By.Name
    // ──────────────────────────────────────────────────────────────────

    private void FindByName()
    {
        Console.WriteLine("\n--- By.Name ---");
        // demoqa practice form has inputs with name attributes
        _driver.Navigate().GoToUrl("https://demoqa.com/automation-practice-form");

        _wait.Until(d => d.FindElement(By.Id("firstName")).Displayed);

        // The student registration form uses name="firstName" etc.
        var firstName = _driver.FindElement(By.Name("firstName"));
        var lastName  = _driver.FindElement(By.Name("lastName"));

        Console.WriteLine($"  [FOUND] name=firstName — tag: <{firstName.TagName}>");
        Console.WriteLine($"  [FOUND] name=lastName  — tag: <{lastName.TagName}>");
    }

    // ──────────────────────────────────────────────────────────────────
    // 3. CSS Selectors — 2 compound examples
    // ──────────────────────────────────────────────────────────────────

    private void FindByCssSelectors()
    {
        Console.WriteLine("\n--- CSS Selectors ---");
        _driver.Navigate().GoToUrl("https://demoqa.com/automation-practice-form");

        _wait.Until(d => d.FindElement(By.Id("firstName")).Displayed);

        // Compound CSS #1:  div.practice-form-wrapper input#firstName
        //   — an <input> with id="firstName" that lives inside a div
        //     with class "practice-form-wrapper"
        var firstNameField = _driver.FindElement(
            By.CssSelector("div.practice-form-wrapper input#firstName"));
        Console.WriteLine($"  [FOUND] div.practice-form-wrapper input#firstName — " +
                          $"placeholder: \"{firstNameField.GetAttribute("placeholder")}\"");

        // Compound CSS #2:  .form-group label[for='gender-radio-1']
        //   — a <label> whose "for" attribute is "gender-radio-1",
        //     inside any element that has class "form-group"
        var maleLabel = _driver.FindElement(
            By.CssSelector(".form-group label[for='gender-radio-1']"));
        Console.WriteLine($"  [FOUND] .form-group label[for='gender-radio-1'] — " +
                          $"text: \"{maleLabel.Text}\"");
    }

    // ──────────────────────────────────────────────────────────────────
    // 4. XPath — 2 compound examples
    // ──────────────────────────────────────────────────────────────────

    private void FindByXPaths()
    {
        Console.WriteLine("\n--- XPath ---");
        _driver.Navigate().GoToUrl("https://demoqa.com/automation-practice-form");

        _wait.Until(d => d.FindElement(By.Id("firstName")).Displayed);

        // Compound XPath #1:
        //   //div[@class='practice-form-wrapper']//input[@id='userEmail']
        //   — input with id "userEmail" anywhere inside that wrapper div
        var emailXPath = _driver.FindElement(
            By.XPath("//div[@class='practice-form-wrapper']//input[@id='userEmail']"));
        Console.WriteLine($"  [FOUND] //div[@class='practice-form-wrapper']//input[@id='userEmail'] — " +
                          $"placeholder: \"{emailXPath.GetAttribute("placeholder")}\"");

        // Compound XPath #2:
        //   //div[contains(@class,'col-md-6')]//label[text()='Current Address']
        //   — a <label> with exact text "Current Address" inside a div
        //     whose class contains "col-md-6"
        //   We use following-sibling or the textarea below it via parent::
        var addressLabel = _driver.FindElement(
            By.XPath("//div[contains(@class,'col-md-6')]//label[text()='Current Address']"));
        Console.WriteLine($"  [FOUND] //div[contains(@class,'col-md-6')]//label[text()='Current Address'] — " +
                          $"text: \"{addressLabel.Text}\"");
    }

    // ──────────────────────────────────────────────────────────────────
    // 5. Partial Link Text
    // ──────────────────────────────────────────────────────────────────

    private void FindByPartialLinkText()
    {
        Console.WriteLine("\n--- Partial Link Text ---");
        _driver.Navigate().GoToUrl("https://demoqa.com/links");

        _wait.Until(d => d.FindElement(By.Id("simpleLink")).Displayed);

        // "Home" link on the page; we search for partial text "Hom"
        var homeLink = _driver.FindElement(By.PartialLinkText("Hom"));
        Console.WriteLine($"  [FOUND] PartialLinkText('Hom') — " +
                          $"full text: \"{homeLink.Text}\", href: \"{homeLink.GetAttribute("href")}\"");

        // Another link — "Created" API response demo link
        var createdLink = _driver.FindElement(By.PartialLinkText("Creat"));
        Console.WriteLine($"  [FOUND] PartialLinkText('Creat') — " +
                          $"full text: \"{createdLink.Text}\"");
    }

    // ──────────────────────────────────────────────────────────────────
    // 6. FindElements — returns IReadOnlyCollection<IWebElement>
    // ──────────────────────────────────────────────────────────────────

    private void FindMultipleElements()
    {
        Console.WriteLine("\n--- FindElements (list) ---");
        _driver.Navigate().GoToUrl("https://demoqa.com/elements");

        // Wait for the sidebar menu to load
        _wait.Until(d => d.FindElement(By.CssSelector(".element-group")).Displayed);

        // Compound CSS — all <li> items inside .element-list inside .element-group
        // This gives us every sidebar menu item (the section groups)
        var menuItems = _driver.FindElements(
            By.CssSelector(".element-group .element-list li.btn"));

        Console.WriteLine($"  [FOUND] {menuItems.Count} sidebar menu items " +
                          $"via '.element-group .element-list li.btn':");

        foreach (var item in menuItems)
            Console.WriteLine($"    • \"{item.Text.Trim()}\"");
    }
}
