using PKB.Application.DTOs;
using PKB.Application.Events;
using PKB.Application.Services;
using PKB.Domain.Entities;
using PKB.Domain.Enums;
using PKB.Lab9.UnitTests.TestDoubles;

namespace PKB.Lab9.UnitTests.Application;

public class KnowledgeItemServiceTests
{
    [Fact]
    public async Task CreateItemAsync_LinkDuplicate_ReturnsDuplicateAndDoesNotPersist()
    {
        var userId = Guid.NewGuid();
        var existing = KnowledgeItem.CreateLink(userId, 1, "https://example.com/dup");

        var itemRepo = new FakeKnowledgeItemRepository
        {
            FindBySourceUrlResult = existing
        };
        var tagRepo = new FakeTagRepository();
        var unitOfWork = new FakeUnitOfWork();
        var background = new FakeBackgroundJobClient();
        var mediator = new FakeMediator();
        var service = new KnowledgeItemService(itemRepo, tagRepo, unitOfWork, background, mediator);

        var request = new CreateItemRequest
        {
            Type = KnowledgeItemType.Link,
            SourceUrl = "https://example.com/dup"
        };

        var result = await service.CreateItemAsync(userId, request);

        Assert.True(result.IsDuplicate);
        Assert.NotNull(result.ExistingItem);
        Assert.Equal(existing.Id, result.ExistingItem!.Id);
        Assert.Equal(0, itemRepo.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(mediator.PublishedNotifications);
    }

    [Fact]
    public async Task CreateItemAsync_Note_PersistsItemAndPublishesEvent()
    {
        var userId = Guid.NewGuid();
        var itemRepo = new FakeKnowledgeItemRepository
        {
            MaxItemNumberResult = 7
        };
        var unitOfWork = new FakeUnitOfWork();
        var mediator = new FakeMediator();
        var service = new KnowledgeItemService(
            itemRepo,
            new FakeTagRepository(),
            unitOfWork,
            new FakeBackgroundJobClient(),
            mediator);
        var request = new CreateItemRequest
        {
            Type = KnowledgeItemType.Note,
            Content = "My note content"
        };

        var result = await service.CreateItemAsync(userId, request);

        Assert.False(result.IsDuplicate);
        Assert.Equal(1, itemRepo.AddCalls);
        Assert.NotNull(itemRepo.LastAddedItem);
        Assert.Equal(8, itemRepo.LastAddedItem!.ItemNumber);
        Assert.Equal(ProcessingStatus.Pending, itemRepo.LastAddedItem.ProcessingStatus);
        Assert.Equal(KnowledgeItemType.Note, itemRepo.LastAddedItem.Type);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Contains(mediator.PublishedNotifications, n => n is ItemCreatedEvent e && e.Type == KnowledgeItemType.Note);
    }

    [Fact]
    public async Task CreateItemAsync_UnsupportedType_ThrowsArgumentException()
    {
        var itemRepo = new FakeKnowledgeItemRepository();
        var unitOfWork = new FakeUnitOfWork();
        var mediator = new FakeMediator();
        var service = new KnowledgeItemService(
            itemRepo,
            new FakeTagRepository(),
            unitOfWork,
            new FakeBackgroundJobClient(),
            mediator);
        var request = new CreateItemRequest
        {
            Type = (KnowledgeItemType)999
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateItemAsync(Guid.NewGuid(), request));
        Assert.Equal(0, itemRepo.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(mediator.PublishedNotifications);
    }

    [Fact]
    public async Task AddTagAsync_NewTag_AddsTagItemLinkSavesAndEnqueuesReindex()
    {
        var itemId = Guid.NewGuid();
        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 1, "content");
        var itemRepo = new FakeKnowledgeItemRepository();
        itemRepo.GetByIdSequence.Enqueue(item);
        itemRepo.GetByIdSequence.Enqueue(item);

        var tagRepo = new FakeTagRepository
        {
            TagByNameResult = null,
            TagsByItemIdResult = Array.Empty<Tag>()
        };
        var unitOfWork = new FakeUnitOfWork();
        var background = new FakeBackgroundJobClient();
        var service = new KnowledgeItemService(itemRepo, tagRepo, unitOfWork, background, new FakeMediator());

        var response = await service.AddTagAsync(itemId, " CSharp ");

        Assert.NotNull(response);
        Assert.Equal(1, tagRepo.AddCalls);
        Assert.Equal(1, tagRepo.AddItemTagCalls);
        Assert.NotNull(tagRepo.LastAddedTag);
        Assert.Equal("csharp", tagRepo.LastAddedTag!.Name);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(background.CreatedJobs);
    }

    [Fact]
    public async Task AddTagAsync_AlreadyAttachedTag_DoesNotCreateNewLink()
    {
        var itemId = Guid.NewGuid();
        var existingTag = new Tag("dev");
        var item = KnowledgeItem.CreateNote(Guid.NewGuid(), 1, "content");
        var itemRepo = new FakeKnowledgeItemRepository();
        itemRepo.GetByIdSequence.Enqueue(item);
        itemRepo.GetByIdSequence.Enqueue(item);

        var tagRepo = new FakeTagRepository
        {
            TagByNameResult = existingTag,
            TagsByItemIdResult = new List<Tag> { existingTag }
        };
        var unitOfWork = new FakeUnitOfWork();
        var background = new FakeBackgroundJobClient();
        var service = new KnowledgeItemService(itemRepo, tagRepo, unitOfWork, background, new FakeMediator());

        var response = await service.AddTagAsync(itemId, "dev");

        Assert.NotNull(response);
        Assert.Equal(0, tagRepo.AddCalls);
        Assert.Equal(0, tagRepo.AddItemTagCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(background.CreatedJobs);
    }

    [Fact]
    public async Task UpdateStatusAsync_ItemNotFound_ReturnsNullAndDoesNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new KnowledgeItemService(
            new FakeKnowledgeItemRepository { ItemByIdResult = null },
            new FakeTagRepository(),
            unitOfWork,
            new FakeBackgroundJobClient(),
            new FakeMediator());

        var result = await service.UpdateStatusAsync(Guid.NewGuid(), ReadingStatus.Completed);

        Assert.Null(result);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task GetStatsAsync_ComputesAggregatedCounts()
    {
        var userId = Guid.NewGuid();
        var itemRepo = new FakeKnowledgeItemRepository
        {
            CountByUserIdResult = 12,
            ImportantItemsResult = new List<KnowledgeItem>
            {
                KnowledgeItem.CreateNote(userId, 1, "i1"),
                KnowledgeItem.CreateNote(userId, 2, "i2")
            },
            UnreadItemsResult = new List<KnowledgeItem>
            {
                KnowledgeItem.CreateNote(userId, 3, "i3"),
                KnowledgeItem.CreateNote(userId, 4, "i4"),
                KnowledgeItem.CreateNote(userId, 5, "i5")
            }
        };
        itemRepo.CountByTypeResults[KnowledgeItemType.Link] = 4;
        itemRepo.CountByTypeResults[KnowledgeItemType.Note] = 6;
        itemRepo.CountByTypeResults[KnowledgeItemType.Pdf] = 2;

        var service = new KnowledgeItemService(
            itemRepo,
            new FakeTagRepository(),
            new FakeUnitOfWork(),
            new FakeBackgroundJobClient(),
            new FakeMediator());

        var stats = await service.GetStatsAsync(userId);

        Assert.Equal(12, stats.TotalItems);
        Assert.Equal(4, stats.LinkCount);
        Assert.Equal(6, stats.NoteCount);
        Assert.Equal(2, stats.PdfCount);
        Assert.Equal(3, stats.UnreadCount);
        Assert.Equal(2, stats.ImportantCount);
    }
}
