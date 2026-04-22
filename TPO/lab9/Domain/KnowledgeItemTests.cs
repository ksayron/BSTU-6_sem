using PKB.Domain.Entities;
using PKB.Domain.Enums;

namespace PKB.Lab9.UnitTests.Domain;

public class KnowledgeItemTests
{
    [Fact]
    public void CreateNote_WithLongContent_TruncatesTitleTo80Chars()
    {
        var content = new string('a', 90);

        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 1, content);

        Assert.Equal(80, item.Title.Length);
        Assert.Equal(content[..80], item.Title);
    }

    [Fact]
    public void CreateNote_WithCustomTitle_UsesProvidedTitle()
    {
        var title = "Explicit title";

        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 1, "Note content", title: title);

        Assert.Equal(title, item.Title);
        Assert.Equal(KnowledgeItemType.Note, item.Type);
    }

    [Fact]
    public void CreateLink_WithoutTitle_UsesSourceUrlAsTitleAndUnreadStatus()
    {
        const string sourceUrl = "https://example.com/article";

        var item = KnowledgeItem.CreateLink(Guid.NewGuid(), 2, sourceUrl);

        Assert.Equal(sourceUrl, item.Title);
        Assert.Equal(sourceUrl, item.SourceUrl);
        Assert.Equal(ReadingStatus.Unread, item.Status);
    }

    [Fact]
    public void ToggleImportant_CalledTwice_ReturnsToInitialState()
    {
        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 3, "Note");

        item.ToggleImportant();
        item.ToggleImportant();

        Assert.False(item.IsImportant);
    }

    [Fact]
    public void SoftDelete_ThenRestore_ClearsDeletedAt()
    {
        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 4, "Note");

        item.SoftDelete();
        item.Restore();

        Assert.Null(item.DeletedAt);
    }

    [Fact]
    public void SetExtractedText_SetsTextAndReadingTime()
    {
        var item = KnowledgeItem.CreateLink(Guid.NewGuid(), 5, "https://example.com");

        item.SetExtractedText("Extracted", 7);

        Assert.Equal("Extracted", item.ExtractedText);
        Assert.Equal(7, item.ReadingTimeMinutes);
    }
}
