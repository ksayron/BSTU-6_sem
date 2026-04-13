# TPO Lab 8 - Selenium UI Tests (GameDev Market)

This lab contains a clean Selenium UI automation project for:

- Target site: `https://www.gamedevmarket.net/`
- Stack: C# + Selenium WebDriver + NUnit
- Pattern: Page Object Model
- Report: Extent HTML report

## Project layout

`TPO/lab8/TPO.Lab8.Tests`

- `Pages/` - Page Objects (`BasePage`, `HomePage`, `SearchResultsPage`, `ProductPage`, `LoginPage`)
- `Helpers/` - driver factory, screenshots, cookies, report, paths
- `Tests/` - NUnit tests and base test setup
- `Artifacts/` - generated screenshots, cookies, reports

## Implemented lab requirements

1. Previous-style scenarios are implemented using Page Object Model.
2. Browser options are configured in `DriverFactory` (maximize, notifications off, optional headless).
3. Cookie capture and export to JSON implemented (`CookieHelper` + `CookieTests`).
4. Screenshots saved in tests and on failures.
5. Parameterized tests implemented in `SearchTests`.
6. Test management features:
- categories/tags (`[Category]`)
- order (`[Order]`)
- skipped/ignored demo test (`[Ignore]`)
- selective test group (`[Category("Auth")]` + `[Explicit]`)
7. Automatic readable HTML report via ExtentReports.

## Credentials

Login test reads credentials from environment variables:

- `GDM_EMAIL`
- `GDM_PASSWORD`

If they are absent, the login test is skipped (`Assert.Ignore`).

## Run instructions

From `TPO/lab8/TPO.Lab8.Tests`:

```powershell
dotnet restore
dotnet test
```

Optional settings:

- `GDM_BROWSER=edge` to use Edge (`chrome` by default)
- `GDM_HEADLESS=1` to run headless

Run only a specific category (example: search):

```powershell
dotnet test --filter "Category=Search"
```

Run login test explicitly:

```powershell
dotnet test --filter "Category=Auth"
```

## Artifacts and report

Generated automatically under:

- `TPO/lab8/TPO.Lab8.Tests/Artifacts/Screenshots`
- `TPO/lab8/TPO.Lab8.Tests/Artifacts/Cookies`
- `TPO/lab8/TPO.Lab8.Tests/Artifacts/Reports`

The Extent report file is `ExtentReport_yyyyMMdd_HHmmss.html`.

## Notes / limitations

- GameDev Market UI can evolve; selectors are intentionally resilient but may still need updates if markup changes.
- Login flow stability depends on account state and any temporary anti-bot checks.
- Some sort/filter controls may have different available options at runtime; the test handles this by skipping when not enough options are present.
