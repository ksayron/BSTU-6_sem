# TPO Lab 8-2 - Selenium UI Tests (SteamBuy)

This is a separate second Lab 8 project for:

- Target site: `https://steambuy.com/`
- Stack: C# + Selenium WebDriver + NUnit + ExtentReports
- Pattern: Page Object Model

## Project path

`TPO/lab8-2/TPO.Lab8_2.Steambuy.Tests`

## Structure

- `Pages/`: `BasePage`, `HomePage`, `CatalogPage`, `SearchResultsPage`, `ProductPage`
- `Helpers/`: `DriverFactory`, `ScreenshotHelper`, `CookieHelper`, `ReportManager`, `PathHelper`
- `Tests/`: `BaseTest`, `SmokeTests`, `SearchTests`, `CatalogFilterTests`, `ProductTests`, `CookieTests`, `TestManagementTests`, `TestRunHooks`
- `Artifacts/`: `Screenshots`, `Cookies`, `Reports` (generated automatically)

## Implemented lab requirements

1. Scenarios are implemented with Page Object Model.
2. Browser options are applied in `DriverFactory` (maximize, notifications off, optional headless).
3. Cookies are read and exported to JSON.
4. Screenshots are saved in scenarios and on failures.
5. Parameterized tests are implemented in `SearchTests`.
6. Test management attributes are used:
- `Category`
- `Order`
- ignored demo test (`Ignore`)
7. Automatic readable HTML report is generated via ExtentReports.

## Run

From `TPO/lab8-2/TPO.Lab8_2.Steambuy.Tests`:

```powershell
dotnet restore
dotnet test
```

Optional environment variables:

- `SB_BROWSER=edge` or `SB_BROWSER=chrome`
- `SB_HEADLESS=1`

Run filtered tests:

```powershell
dotnet test --filter "Category=Search"
dotnet test --filter "Category=Smoke"
dotnet test --filter "Category=Catalog"
```

## Report and artifacts

Generated in:

- `TPO/lab8-2/TPO.Lab8_2.Steambuy.Tests/Artifacts/Reports`
- `TPO/lab8-2/TPO.Lab8_2.Steambuy.Tests/Artifacts/Screenshots`
- `TPO/lab8-2/TPO.Lab8_2.Steambuy.Tests/Artifacts/Cookies`

Report file format: `ExtentReport_yyyyMMdd_HHmmss.html`.

To open the latest report from PowerShell:

```powershell
Get-ChildItem .\Artifacts\Reports\*.html | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | ForEach-Object { Start-Process $_.FullName }
```

## Known limitations

- SteamBuy UI and catalog markup can change over time.
- The site can occasionally show anti-bot or challenge pages; tests skip safely in that case instead of producing false failures.
- Search/filter behavior can be dynamic depending on current catalog content and query.
