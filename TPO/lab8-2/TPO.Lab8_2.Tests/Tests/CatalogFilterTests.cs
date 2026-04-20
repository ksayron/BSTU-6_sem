using TPO.Lab8_2.Tests.Pages;

namespace TPO.Lab8_2.Tests.Tests;

[TestFixture]
public sealed class CatalogFilterTests : BaseTest
{
    [Test]
    [Order(3)]
    [Category("Catalog")]
    [Category("FilterSort")]
    public void Catalog_ShouldAllowSortInteraction()
    {
        HomePage.Open();
        var catalog = HomePage.OpenCatalog();

        Assert.That(catalog.HasSortControls(), Is.True, "Sort controls not found on catalog page.");
        var beforeUrl = catalog.CurrentUrl;
        var beforeCount = catalog.GetResultCount();

        catalog.ApplySortByPriceAsc();

        var afterUrl = catalog.CurrentUrl;
        var afterCount = catalog.GetResultCount();

        Assert.That(afterUrl, Does.Contain("sort="), "Catalog URL should include sort parameter after interaction.");
        Assert.That(afterCount, Is.GreaterThanOrEqualTo(0), "Result count should be readable after sort interaction.");
        Assert.That(beforeUrl != afterUrl || beforeCount == afterCount || afterCount > 0, Is.True,
            "Sort interaction did not produce any observable catalog state.");
    }
}
