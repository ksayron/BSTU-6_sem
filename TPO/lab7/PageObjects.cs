using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumLab.Task2.Pages;

// ──────────────────────────────────────────────────────────────────────────────
// Base Page — shared driver + wait infrastructure
// Every page object inherits from this.
// ──────────────────────────────────────────────────────────────────────────────

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait   = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    /// <summary>Scroll element into view then click — avoids "element not interactable"
    /// when something is hidden behind the sticky header on demoqa.</summary>
    protected void ScrollAndClick(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'center'});", element);
        element.Click();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Practice Form Page  —  https://demoqa.com/automation-practice-form
// ──────────────────────────────────────────────────────────────────────────────

public class PracticeFormPage : BasePage
{
    public const string Url = "https://demoqa.com/automation-practice-form";

    // Locators
    private IWebElement FirstName    => Driver.FindElement(By.Id("firstName"));
    private IWebElement LastName     => Driver.FindElement(By.Id("lastName"));
    private IWebElement Email        => Driver.FindElement(By.Id("userEmail"));
    private IWebElement MobileNumber => Driver.FindElement(By.Id("userNumber"));
    private IWebElement SubmitButton => Driver.FindElement(By.Id("submit"));

    // Radio buttons — demoqa uses labels that wrap hidden inputs,
    // so we click the <label> rather than the <input>
    private IWebElement GenderMaleLabel =>
        Driver.FindElement(By.CssSelector("label[for='gender-radio-1']"));
    private IWebElement GenderFemaleLabel =>
        Driver.FindElement(By.CssSelector("label[for='gender-radio-2']"));

    // Checkboxes (hobbies)
    private IWebElement HobbySportsLabel =>
        Driver.FindElement(By.CssSelector("label[for='hobbies-checkbox-1']"));
    private IWebElement HobbyReadingLabel =>
        Driver.FindElement(By.CssSelector("label[for='hobbies-checkbox-2']"));
    private IWebElement HobbyMusicLabel  =>
        Driver.FindElement(By.CssSelector("label[for='hobbies-checkbox-3']"));

    // State/City dropdowns (react-select)
    private IWebElement StateDropdown =>
        Driver.FindElement(By.Id("state"));
    private IWebElement StateInput =>
        Driver.FindElement(By.CssSelector("#state input"));
    private IWebElement CityDropdown =>
        Driver.FindElement(By.Id("city"));
    private IWebElement CityInput =>
        Driver.FindElement(By.CssSelector("#city input"));

    // Result modal
    public IWebElement ConfirmationModal =>
        Wait.Until(d => d.FindElement(By.Id("example-modal-sizes-title-lg")));
    public IWebElement CloseModalButton =>
        Driver.FindElement(By.Id("closeLargeModal"));

    // Returns every row in the confirmation table as (label, value)
    public IReadOnlyList<(string Label, string Value)> GetConfirmationRows()
    {
        var rows = Driver.FindElements(By.CssSelector("#example-modal-sizes-title-lg ~ .modal-body table tbody tr"));
        return rows
            .Select(r =>
            {
                var cells = r.FindElements(By.TagName("td"));
                return (cells[0].Text.Trim(), cells[1].Text.Trim());
            })
            .ToList();
    }

    public PracticeFormPage(IWebDriver driver) : base(driver) { }

    public void Open()
    {
        Driver.Navigate().GoToUrl(Url);
        Wait.Until(d => d.FindElement(By.Id("firstName")).Displayed);
    }

    public void EnterFirstName(string value)  => FirstName.SendKeys(value);
    public void EnterLastName(string value)   => LastName.SendKeys(value);
    public void EnterEmail(string value)      => Email.SendKeys(value);
    public void EnterMobileNumber(string value) => MobileNumber.SendKeys(value);

    public void SelectGenderMale()   => ScrollAndClick(GenderMaleLabel);
    public void SelectGenderFemale() => ScrollAndClick(GenderFemaleLabel);

    public void CheckHobbySports()  => ScrollAndClick(HobbySportsLabel);
    public void CheckHobbyReading() => ScrollAndClick(HobbyReadingLabel);
    public void CheckHobbyMusic()   => ScrollAndClick(HobbyMusicLabel);

    /// <summary>Select a State from the react-select dropdown by typing its name.</summary>
    public void SelectState(string stateName)
    {
        ScrollAndClick(StateDropdown);
        StateInput.SendKeys(stateName);
        // Wait for the option to appear and click it
        var option = Wait.Until(d => d.FindElement(
            By.XPath($"//div[contains(@class,'option') and text()='{stateName}']")));
        option.Click();
    }

    /// <summary>Select a City from the react-select dropdown by typing its name.</summary>
    public void SelectCity(string cityName)
    {
        ScrollAndClick(CityDropdown);
        CityInput.SendKeys(cityName);
        var option = Wait.Until(d => d.FindElement(
            By.XPath($"//div[contains(@class,'option') and text()='{cityName}']")));
        option.Click();
    }

    public void ClickSubmit() => ScrollAndClick(SubmitButton);
}

// ──────────────────────────────────────────────────────────────────────────────
// Text Box Page  —  https://demoqa.com/text-box
// ──────────────────────────────────────────────────────────────────────────────

public class TextBoxPage : BasePage
{
    public const string Url = "https://demoqa.com/text-box";

    private IWebElement FullNameInput      => Driver.FindElement(By.Id("userName"));
    private IWebElement EmailInput         => Driver.FindElement(By.Id("userEmail"));
    private IWebElement CurrentAddressBox  => Driver.FindElement(By.Id("currentAddress"));
    private IWebElement PermanentAddressBox => Driver.FindElement(By.Id("permanentAddress"));
    private IWebElement SubmitButton        => Driver.FindElement(By.Id("submit"));

    // Output block shown after submission
    private IWebElement OutputBlock =>
        Wait.Until(d => d.FindElement(By.Id("output")));

    public string OutputName =>
        OutputBlock.FindElement(By.Id("name")).Text;
    public string OutputEmail =>
        OutputBlock.FindElement(By.Id("email")).Text;

    public TextBoxPage(IWebDriver driver) : base(driver) { }

    public void Open()
    {
        Driver.Navigate().GoToUrl(Url);
        Wait.Until(d => d.FindElement(By.Id("userName")).Displayed);
    }

    public void Fill(string fullName, string email,
                     string currentAddress, string permanentAddress)
    {
        FullNameInput.Clear();
        FullNameInput.SendKeys(fullName);

        EmailInput.Clear();
        EmailInput.SendKeys(email);

        CurrentAddressBox.Clear();
        CurrentAddressBox.SendKeys(currentAddress);

        PermanentAddressBox.Clear();
        PermanentAddressBox.SendKeys(permanentAddress);
    }

    public void ClickSubmit() => ScrollAndClick(SubmitButton);
}

// ──────────────────────────────────────────────────────────────────────────────
// Checkbox Page  —  https://demoqa.com/checkbox
// ──────────────────────────────────────────────────────────────────────────────

public class CheckboxPage : BasePage
{
    public const string Url = "https://demoqa.com/checkbox";

    private IWebElement ExpandAllButton =>
        Driver.FindElement(By.CssSelector("button[title='Expand all']"));

    private IWebElement ResultBlock =>
        Wait.Until(d => d.FindElement(By.Id("result")));

    public IReadOnlyList<string> SelectedItems =>
        ResultBlock.FindElements(By.TagName("span"))
                   .Where(s => s.GetAttribute("class") == "text-success")
                   .Select(s => s.Text)
                   .ToList();

    public CheckboxPage(IWebDriver driver) : base(driver) { }

    public void Open()
    {
        Driver.Navigate().GoToUrl(Url);
        Wait.Until(d => d.FindElement(By.CssSelector(".rct-node")).Displayed);
    }

    public void ExpandAll() => ExpandAllButton.Click();

    /// <summary>Click the checkbox label matching the given node title (case-insensitive).</summary>
    public void CheckItemByLabel(string label)
    {
        var labelElement = Wait.Until(d =>
            d.FindElements(By.CssSelector(".rct-node .rct-title"))
             .FirstOrDefault(e => e.Text.Equals(label, StringComparison.OrdinalIgnoreCase)));

        if (labelElement == null)
            throw new InvalidOperationException($"Checkbox label '{label}' not found");

        ScrollAndClick(labelElement);
    }
}
