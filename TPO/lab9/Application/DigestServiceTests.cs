using PKB.Application.Services;
using PKB.Domain.Entities;
using PKB.Domain.Enums;
using PKB.Lab9.UnitTests.TestDoubles;

namespace PKB.Lab9.UnitTests.Application;

public class DigestServiceTests
{
    [Fact]
    public async Task GenerateDigestAsync_WithMixedItems_ComputesCountsAndTopTags()
    {
        var userId = Guid.NewGuid();
        var link = KnowledgeItem.CreateLink(userId, 1, "https://example.com");
        link.UpdateStatus(ReadingStatus.Unread);
        link.SetExtractedText("text", 5);

        var note = KnowledgeItem.CreateNote(userId, 2, "note");
        note.UpdateStatus(ReadingStatus.Completed);
        note.SetExtractedText("text", 10);

        var pdf = KnowledgeItem.CreatePdf(userId, 3, "doc.pdf");
        pdf.UpdateStatus(ReadingStatus.Unread);

        var itemRepo = new FakeKnowledgeItemRepository
        {
            ItemsSinceResult = new List<KnowledgeItem> { link, note, pdf }
        };
        var tagRepo = new FakeTagRepository
        {
            TopTagsByUserResult = new List<(string TagName, int Count)>
            {
                ("ai", 3),
                ("dotnet", 2)
            }
        };
        var service = new DigestService(itemRepo, tagRepo);

        var digest = await service.GenerateDigestAsync(userId);

        Assert.Equal(3, digest.TotalItems);
        Assert.Equal(1, digest.ArticleCount);
        Assert.Equal(1, digest.NoteCount);
        Assert.Equal(1, digest.PdfCount);
        Assert.Equal(2, digest.UnreadCount);
        Assert.Equal(15, digest.EstimatedReadingTimeMinutes);
        Assert.Equal(2, digest.TopTags.Count);
        Assert.Equal("ai", digest.TopTags[0].Name);
        Assert.Equal(3, digest.TopTags[0].Count);
        Assert.True(digest.PeriodEnd >= digest.PeriodStart);
    }

    [Fact]
    public async Task GenerateDigestAsync_WithNoItems_ReturnsZeroCounts()
    {
        var service = new DigestService(
            new FakeKnowledgeItemRepository
            {
                ItemsSinceResult = Array.Empty<KnowledgeItem>()
            },
            new FakeTagRepository
            {
                TopTagsByUserResult = Array.Empty<(string TagName, int Count)>()
            });

        var digest = await service.GenerateDigestAsync(Guid.NewGuid());

        Assert.Equal(0, digest.TotalItems);
        Assert.Equal(0, digest.ArticleCount);
        Assert.Equal(0, digest.NoteCount);
        Assert.Equal(0, digest.PdfCount);
        Assert.Equal(0, digest.UnreadCount);
        Assert.Equal(0, digest.EstimatedReadingTimeMinutes);
        Assert.Empty(digest.TopTags);
    }
}
