// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DirectiveAthenaWeb.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentRepositoryBase<T>(IContentStorage contentStorage, ILogger logger) : IContentRepository<T>
    where T : ContentBase, IContent {
    
    private ConcurrentDictionary<Guid, T> Items { get; set; } = [];
    private bool _isFirstTimeLoaded; // TODO needs a semaphoreslim
    
    private EntityTagHeaderValue? _etag;
    private DateTimeOffset? _lastModifiedUtc;
    private DateTimeOffset? _lastRefreshUtc;
    #if DEBUG
    private readonly TimeSpan DevRefreshWindow = TimeSpan.FromSeconds(5);
    #else
    private readonly TimeSpan CacheRefreshWindow = TimeSpan.FromMinutes(5);
    #endif

    protected abstract JsonTypeInfo<T[]> ContentListTypeInfo { get; }

    // -----------------------------------------------------------------------------------------------------------------
    // CRUD Methods
    // -----------------------------------------------------------------------------------------------------------------
    #region CRUD Methods
    public async ValueTask<T[]> GetAllAsync(QueryConfig config = default, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);

        IEnumerable<T> query = GetConfiguredQuery(Items.Values, config);
        T[] results = query.ToArray();
        return results;
    }

    public async ValueTask<T?> GetByIdAsync(Guid id, QueryConfig config = default, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);

        T? item = Items.GetValueOrDefault(id);
        if (item is null) return null;

        IEnumerable<T> query = GetConfiguredQuery([item], config);
        return query.FirstOrDefault();
    }

    public async ValueTask<bool> SoftDeleteByIdAsync(Guid id, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);

        if (!Items.TryGetValue(id, out T? item)) {
            logger.Warning("{ContentType} {Id} not found for soft deletion.", typeof(T).Name, id);
            return false;
        }

        if (item.IsSoftDeleted) {
            logger.Debug("{ContentType} {Id} already soft deleted.", typeof(T).Name, id);
            return true;
        }

        DateTime now = DateTime.UtcNow;
        item.SoftDeletedAt = now;
        item.LastModifiedAt = now;

        logger.Information("Soft deleted {ContentType} {Id}", typeof(T).Name, id);
        return true;
    }

    public async ValueTask<bool> HardDeleteByIdAsync(Guid id, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);
        if (!Items.TryGetValue(id, out T? item)) {
            logger.Warning("{ContentType} {Id} not found for deletion.", typeof(T).Name, id);
            return false;
        }

        bool allDeleted = await contentStorage.DeleteLocalizedFilesAsync(item.MarkdownFileName, ct);
        if (!allDeleted) {
            logger.Warning("Failed to delete localized files for {ContentType} {Id}.", typeof(T).Name, id);
            return false;
        }

        if (!Items.TryRemove(id, out _)) {
            logger.Warning("{ContentType} {Id} not found for deletion.", typeof(T).Name, id);
            return false;
        }
        
        logger.Information("Deleted {ContentType} {Id}", typeof(T).Name, id);
        return true;
    }
    
    public async ValueTask<bool> RestoreByIdAsync(Guid id, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);
        
        if (!Items.TryGetValue(id, out T? item)) {
            logger.Warning("{ContentType} {Id} not found for soft deletion.", typeof(T).Name, id);
            return false;
        }
        
        if (!item.IsSoftDeleted) {
            logger.Debug("{ContentType} {Id} already restored.", typeof(T).Name, id);
            return true;
        }

        item.SoftDeletedAt = DateTime.MinValue;
        item.LastModifiedAt = DateTime.UtcNow;

        logger.Information("Restored {ContentType} {Id}", typeof(T).Name, id);
        return true;
    }
    
    public async ValueTask<bool> AddOrUpdateAsync(T item, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);
        
        try {
            item.LastModifiedAt = DateTime.UtcNow;
            Items.AddOrUpdate(item.Id, item, (_, _) => item);
            logger.Information("Added or updated {ContentType} {Id}", typeof(T).Name, item.Id);
            return true;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to add or update {ContentType} {Id}", typeof(T).Name, item.Id);
            return false;
        }
    }
    public async ValueTask<bool> AddOrUpdateRangeAsync(IEnumerable<T> items, CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);

        bool state = true;
        foreach (T item in items) {
            try {
                item.LastModifiedAt = DateTime.UtcNow;
                Items.AddOrUpdate(item.Id, item, (_, _) => item);
                logger.Information("Added or updated {ContentType} {Id}", typeof(T).Name, item.Id);
            }
            catch (Exception e) {
                logger.Error(e, "Failed to add or update {ContentType} {Id}", typeof(T).Name, item.Id);
                state = false;
            }
        }
        
        return state;
    }
    #endregion

    public async ValueTask<bool> SaveAsync(CancellationToken ct = default) {
        if (!_isFirstTimeLoaded) return false;

        string json = await GetAsJsonStringAsync(ct);
        return await contentStorage.WriteIndexAsync(json, ct);
    }

    public async ValueTask<string> GetAsJsonStringAsync(CancellationToken ct = default) {
        await EnsureDataIsLoadedAsync(ct);

        T[] itemList = Items.Values
            .OrderBy(item => item.Id)
            .ToArray();

        await using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, itemList, ContentListTypeInfo, ct);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static IEnumerable<T> GetConfiguredQuery(IEnumerable<T> data, QueryConfig config) {
        IEnumerable<T> query = data;
        
        // Filters work because they go from a complete dataset to a smaller subset
        //      Meaning that we can't "add" by the filter, because that would make the filters not behave like expected
        if (!config.HasFlagFast(QueryConfig.WithHidden)) query = query.Where(item => !item.IsHidden);
        if (!config.HasFlagFast(QueryConfig.WithSoftDeleted)) query = query.Where(item => !item.IsSoftDeleted);
        if (!config.HasFlagFast(QueryConfig.WithDevContent)) query = query.Where(item => !item.IsDevContent);
        
        bool sortByCreated = config.HasFlagFast(QueryConfig.SortByCreatedAt);
        bool sortByModified = config.HasFlagFast(QueryConfig.SortByModifiedAt);
        bool sortByInternalTitle = config.HasFlagFast(QueryConfig.SortByInternalTitle);
        bool reversed = config.HasFlagFast(QueryConfig.Reversed);

        IOrderedEnumerable<T> ordered = (sortByCreated, sortByModified, sortByInternalTitle, reversed) switch {
            // Both timestamps: use a compound key.
            (true,  true,  _,     false) => query.OrderBy(i => i.LastModifiedAt).ThenBy(i => i.CreatedAt),
            (true,  true,  _,     true)  => query.OrderByDescending(i => i.LastModifiedAt).ThenByDescending(i => i.CreatedAt),

            // Single-key sorts
            (false, true,  _,     false) => query.OrderBy(i => i.LastModifiedAt),
            (false, true,  _,     true)  => query.OrderByDescending(i => i.LastModifiedAt),

            (true,  false, _,     false) => query.OrderBy(i => i.CreatedAt),
            (true,  false, _,     true)  => query.OrderByDescending(i => i.CreatedAt),

            (false, false, true,  false) => query.OrderBy(i => i.InternalTitle),
            (false, false, true,  true)  => query.OrderByDescending(i => i.InternalTitle),

            // Default
            (false, false, false, false) => query.OrderBy(i => i.Id),
            (false, false, false, true)  => query.OrderByDescending(i => i.Id),
        };
        
        // Always stable tie-break
        ordered = reversed
            ? ordered.ThenByDescending(i => i.Id)
            : ordered.ThenBy(i => i.Id);

        query = ordered;

        return query;
    }

    private async ValueTask EnsureDataIsLoadedAsync(CancellationToken ct) {
        try {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            #if DEBUG
            // ReSharper disable once InlineTemporaryVariable
            TimeSpan window = DevRefreshWindow;
            #else
            // ReSharper disable once InlineTemporaryVariable
            TimeSpan window = CacheRefreshWindow;
            #endif

            bool shouldRefresh;
            if (!_isFirstTimeLoaded || _lastRefreshUtc is null) shouldRefresh = true;
            else shouldRefresh = now - _lastRefreshUtc.Value > window;

            if (_isFirstTimeLoaded && !shouldRefresh) {
                logger.Debug("{ContentType} cache was refreshed by another caller.", typeof(T).Name);
                return;
            }

            ContentReadResult response = await contentStorage.ReadIndexAsync(_etag, _lastModifiedUtc, ct);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (response.StatusCode) {
                case HttpStatusCode.OK: break;

                case HttpStatusCode.NotFound:
                    logger.Warning("{ContentType} index not found at {Path}; treating as empty dataset.", typeof(T).Name, contentStorage.IndexContentPath);
                    SetItemsToFaultedState();
                    return;

                case HttpStatusCode.NotModified when _isFirstTimeLoaded: {
                    _lastRefreshUtc = now;
                    if (response.ETag is not null) _etag = response.ETag;
                    if (response.LastModifiedUtc is not null) _lastModifiedUtc = response.LastModifiedUtc;
                    logger.Debug("{ContentType} cache not modified; updated refresh markers.", typeof(T).Name);
                    return;
                }

                default: {
                    logger.Warning("Failed to load {ContentType} index: {StatusCode}.", typeof(T).Name, response.StatusCode);
                    SetItemsToFaultedState();
                    return;
                }
            }

            T[] items = response.Content.IsNotNullOrWhiteSpace()
                ? JsonSerializer.Deserialize(response.Content, ContentListTypeInfo) ?? Array.Empty<T>()
                : Array.Empty<T>();

            NormalizeMissingTimestamps(items, DateTime.UtcNow);
            Items = new ConcurrentDictionary<Guid, T>(items.ToDictionary(item => item.Id, item => item));
            _isFirstTimeLoaded = true;
            _etag = response.ETag;
            _lastModifiedUtc = response.LastModifiedUtc;
            _lastRefreshUtc = now;
            logger.Information("Loaded {Count} {ContentType} items into cache.", Items.Count, typeof(T).Name);
        }
        catch (Exception ex) {
            logger.Error(ex, "Failed to refresh {ContentType} cache; clearing any cached data.", typeof(T).Name);
            SetItemsToFaultedState();
        }
    }

    private static void NormalizeMissingTimestamps(T[] items, DateTime nowUtc) {
        foreach (T item in items) {
            if (item.CreatedAt == default) {
                item.CreatedAt = nowUtc;
            }

            if (item.LastModifiedAt == default) {
                item.LastModifiedAt = item.CreatedAt;
            }

            if (item.IsSoftDeleted && item.SoftDeletedAt == default) {
                item.SoftDeletedAt = item.LastModifiedAt == default ? nowUtc : item.LastModifiedAt;
            }
        }
    }

    private void SetItemsToFaultedState() {
        Items = [];
        _isFirstTimeLoaded = true;
        _etag = null;
        _lastModifiedUtc = null;
        _lastRefreshUtc = null;
    }
}
