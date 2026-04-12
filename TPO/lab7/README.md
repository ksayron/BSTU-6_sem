# SeleniumLab — C# Selenium Assignment

Demo site: **https://demoqa.com** (ToolsQA practice site — no captchas, stable IDs)

## Project structure

```
SeleniumLab/
├── Program.cs                          ← entry point (runs both tasks)
├── SeleniumLab.csproj
├── Task1_ElementFinding/
│   └── ElementFindingDemo.cs           ← Task 1
└── Task2/
    ├── Pages/
    │   └── PageObjects.cs              ← Page Object Model (BasePage, PracticeFormPage, …)
    └── Tests.cs                        ← Task 2 test cases
```

## Setup & run

### Prerequisites
- .NET 8 SDK  (`dotnet --version`)
- Google Chrome installed

### Run

```bash
cd SeleniumLab
dotnet run
```

`Selenium.WebDriver.ChromeDriver` NuGet package auto-downloads the matching
ChromeDriver binary at build time — no manual driver download needed.

---

## Task 1 — Element finding (ElementFindingDemo.cs)

| Locator strategy            | Where used                       |
|-----------------------------|----------------------------------|
| `By.Id`                     | `#userName`, `#userEmail` on Text Box page |
| `By.Name`                   | `name=firstName`, `name=lastName` on Practice Form |
| CSS selector (compound) #1  | `div.practice-form-wrapper input#firstName` |
| CSS selector (compound) #2  | `.form-group label[for='gender-radio-1']` |
| XPath (compound) #1         | `//div[@class='practice-form-wrapper']//input[@id='userEmail']` |
| XPath (compound) #2         | `//div[contains(@class,'col-md-6')]//label[text()='Current Address']` |
| `By.PartialLinkText`        | `"Hom"` → "Home" link on Links page |
| `FindElements` (list)       | All sidebar `li.btn` items on Elements page |

---

## Task 2 — Tests (Tests.cs)

| Test                 | What it covers                                                     |
|----------------------|--------------------------------------------------------------------|
| **TC1** Text Box     | Fill form, submit, verify name+email in output block              |
| **TC2** Checkbox tree | Expand all nodes, check "Downloads", verify result panel          |
| **TC3** Widgets      | Radio button (Female), checkbox (Music), react-select dropdowns    |
| **E2E** Registration | Complete student registration → verify all fields in modal         |

Both **implicit** and **explicit** waits are used:
- Implicit: `driver.Manage().Timeouts().ImplicitWait = 5s` in `SetupDriver()`
- Explicit: `WebDriverWait` + `wait.Until(...)` for specific conditions (modal appearing, dropdown option loading, etc.)

Every test ends with an assertion comparing **actual vs expected** result.

---

## Why demoqa.com?

- No login / captcha required for most features
- Stable element IDs (ideal for By.Id)
- Has radio buttons, checkboxes, react-select dropdowns, tree checkboxes, forms
- Practice Form gives a confirmation modal — perfect for end-to-end assertions
