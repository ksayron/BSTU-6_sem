using TPO.Lab8_2.Tests.Helpers;
using TPO.Lab8_2.Tests.Pages;

namespace TPO.Lab8_2.Tests.Tests;

[TestFixture]
public sealed class ProductTests : BaseTest
{
    [Test]
    [Order(4)]
    [Category("Product")]
    public void ProductPage_ShouldDisplayKeyInformation()
    {
        HomePage.Open();
        var results = HomePage.Search("Resident Evil");

        ProductPage productPage;
        if (results.GetResultsCount() > 0)
        {
            productPage = results.OpenFirstResult();
        }
        else
        {
            productPage = new ProductPage(Driver);
            productPage.OpenKnownProduct();
        }

        Assert.That(productPage.GetTitle(), Is.Not.Empty, "Product title is missing.");
        Assert.That(productPage.HasPriceLikeValue(), Is.True, "Price-like value was not found.");
        Assert.That(productPage.HasKeyDetails(), Is.True, "Key product details were not found.");

        var screenshot = ScreenshotHelper.SaveScreenshot(Driver, "product_page");
        ReportManager.AttachScreenshot(screenshot, "Product details");
    }
}
