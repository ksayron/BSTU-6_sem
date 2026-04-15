using TPO.Lab8.Tests.Helpers;

namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class ProductTests : BaseTest
{
    [Test]
    [Order(2)]
    [Category("Product")]
    public void OpenProductFromSearch_ShouldShowTitleAndMainInfo()
    {
        HomePage.Open();
        var results = HomePage.Search("rpg");
        var product = results.OpenFirstProduct();

        var title = product.GetTitle();
        Assert.That(title, Is.Not.Empty, "Product title is empty.");
        Assert.That(Driver.Url, Does.Contain("/asset/"), "Expected to open an asset page.");
        Assert.That(product.HasPriceOrFreeIndicator(), Is.True, "Price or free indicator is missing.");
        Assert.That(product.HasCategoryInformation(), Is.True, "Category/breadcrumb info was not detected.");

        var screenshotPath = ScreenshotHelper.SaveScreenshot(Driver, "product_details");
        ReportManager.AttachScreenshot(screenshotPath, "Product details page");
    }
}
