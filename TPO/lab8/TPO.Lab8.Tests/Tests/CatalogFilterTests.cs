namespace TPO.Lab8.Tests.Tests;

[TestFixture]
public sealed class CatalogFilterTests : BaseTest
{
    [Test]
    [Order(3)]
    [Category("Catalog")]
    [Category("FilterSort")]
    public void SearchResults_OrderBySelection_ShouldChangeSelectedValue()
    {
        HomePage.Open();
        var results = HomePage.Search("free");

        var optionsCount = results.GetOrderByOptionsCount();
        if (optionsCount < 2)
        {
            Assert.Ignore("Order by control does not have enough options to change selection.");
        }

        var before = results.GetSelectedOrderByText();
        results.SelectOrderByIndex(1);
        var after = results.GetSelectedOrderByText();

        Assert.That(after, Is.Not.EqualTo(before), "Sort value did not change.");
        Assert.That(results.GetResultCount(), Is.GreaterThan(0), "No results after changing sort.");
    }
}
