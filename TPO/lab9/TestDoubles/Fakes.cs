using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MediatR;
using PKB.Domain.Entities;
using PKB.Domain.Enums;
using PKB.Domain.Interfaces;

namespace PKB.Lab9.UnitTests.TestDoubles;

internal sealed class FakeKnowledgeItemRepository : IKnowledgeItemRepository
{
    public KnowledgeItem? ItemByIdResult { get; set; }
    public KnowledgeItem? ItemByUserItemNumberResult { get; set; }
    public KnowledgeItem? FindBySourceUrlResult { get; set; }
    public KnowledgeItem? RandomByUserIdResult { get; set; }
    public int MaxItemNumberResult { get; set; }
    public int CountByUserIdResult { get; set; }
    public Dictionary<KnowledgeItemType, int> CountByTypeResults { get; } = new();
    public IReadOnlyList<KnowledgeItem> UserItemsResult { get; set; } = Array.Empty<KnowledgeItem>();
    public IReadOnlyList<KnowledgeItem> UnreadItemsResult { get; set; } = Array.Empty<KnowledgeItem>();
    public IReadOnlyList<KnowledgeItem> ImportantItemsResult { get; set; } = Array.Empty<KnowledgeItem>();
    public IReadOnlyList<KnowledgeItem> DeletedItemsResult { get; set; } = Array.Empty<KnowledgeItem>();
    public IReadOnlyList<KnowledgeItem> ItemsSinceResult { get; set; } = Array.Empty<KnowledgeItem>();
    public Queue<KnowledgeItem?> GetByIdSequence { get; } = new();

    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public KnowledgeItem? LastAddedItem { get; private set; }

    public Task<KnowledgeItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (GetByIdSequence.Count > 0)
        {
            return Task.FromResult(GetByIdSequence.Dequeue());
        }

        return Task.FromResult(ItemByIdResult);
    }

    public Task<KnowledgeItem?> GetByUserItemNumberAsync(Guid userId, int itemNumber, CancellationToken ct = default)
        => Task.FromResult(ItemByUserItemNumberResult);

    public Task<int> GetMaxItemNumberAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(MaxItemNumberResult);

    public Task<IReadOnlyList<KnowledgeItem>> GetByUserIdAsync(Guid userId, ReadingStatus? status = null, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        if (status == ReadingStatus.Unread)
        {
            return Task.FromResult(UnreadItemsResult);
        }

        return Task.FromResult(UserItemsResult);
    }

    public Task<int> CountByUserIdAsync(Guid userId, DateTime? since = null, CancellationToken ct = default)
        => Task.FromResult(CountByUserIdResult);

    public Task<IReadOnlyList<KnowledgeItem>> GetByUserIdSinceAsync(Guid userId, DateTime since, CancellationToken ct = default)
        => Task.FromResult(ItemsSinceResult);

    public Task<KnowledgeItem?> GetRandomByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(RandomByUserIdResult);

    public Task<IReadOnlyList<KnowledgeItem>> GetImportantByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken ct = default)
        => Task.FromResult(ImportantItemsResult);

    public Task<IReadOnlyList<KnowledgeItem>> GetDeletedByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken ct = default)
        => Task.FromResult(DeletedItemsResult);

    public Task<KnowledgeItem?> FindBySourceUrlAsync(Guid userId, string sourceUrl, CancellationToken ct = default)
        => Task.FromResult(FindBySourceUrlResult);

    public Task<int> CountByTypeAsync(Guid userId, KnowledgeItemType type, CancellationToken ct = default)
    {
        CountByTypeResults.TryGetValue(type, out var value);
        return Task.FromResult(value);
    }

    public Task AddAsync(KnowledgeItem item, CancellationToken ct = default)
    {
        AddCalls++;
        LastAddedItem = item;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(KnowledgeItem item, CancellationToken ct = default)
    {
        UpdateCalls++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeTagRepository : ITagRepository
{
    public Tag? TagByNameResult { get; set; }
    public IReadOnlyList<Tag> TagsByItemIdResult { get; set; } = Array.Empty<Tag>();
    public IReadOnlyList<(string TagName, int Count)> TopTagsByUserResult { get; set; } = Array.Empty<(string TagName, int Count)>();

    public int AddCalls { get; private set; }
    public int AddItemTagCalls { get; private set; }
    public Tag? LastAddedTag { get; private set; }
    public ItemTag? LastAddedItemTag { get; private set; }

    public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
        => Task.FromResult(TagByNameResult);

    public Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        AddCalls++;
        LastAddedTag = tag;
        return Task.CompletedTask;
    }

    public Task AddItemTagAsync(ItemTag itemTag, CancellationToken ct = default)
    {
        AddItemTagCalls++;
        LastAddedItemTag = itemTag;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Tag>> GetTagsByItemIdAsync(Guid itemId, CancellationToken ct = default)
        => Task.FromResult(TagsByItemIdResult);

    public Task<IReadOnlyList<(string TagName, int Count)>> GetTopTagsByUserAsync(Guid userId, int count = 5, DateTime? since = null, CancellationToken ct = default)
        => Task.FromResult(TopTagsByUserResult);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeBackgroundJobClient : IBackgroundJobClient
{
    public List<Job> CreatedJobs { get; } = new();

    public string Create(Job job, IState state)
    {
        CreatedJobs.Add(job);
        return $"job-{CreatedJobs.Count}";
    }

    public bool ChangeState(string jobId, IState state, string expectedState)
        => true;
}

internal sealed class FakeMediator : IMediator
{
    public List<object> PublishedNotifications { get; } = new();

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        PublishedNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        PublishedNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Send is not used in these tests.");

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
        => throw new NotSupportedException("Send is not used in these tests.");

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Send is not used in these tests.");

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("CreateStream is not used in these tests.");

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("CreateStream is not used in these tests.");
}
