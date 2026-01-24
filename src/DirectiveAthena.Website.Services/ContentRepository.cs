// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.FileSystem;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentRepository<T>(HttpClient http, IContentStorage contentStorage) : IContentRepository<T>
    where T : ContentBase {
    private readonly SemaphoreSlim _lock = new(1, 1);
    protected IContentStorage Storage { get; } = contentStorage;
    protected ImmutableDictionary<Guid, T> ItemsById { get; private set; } = ImmutableDictionary<Guid, T>.Empty;
    private bool _hasLoaded;
    private EntityTagHeaderValue? _etag;
    private DateTimeOffset? _lastModifiedUtc;
    private DateTimeOffset? _lastRefreshUtc;
    private readonly TimeSpan CacheRefreshWindow = TimeSpan.FromMinutes(5);
    private readonly TimeSpan DevRefreshWindow = TimeSpan.FromSeconds(5);

    protected abstract string IndexPath { get; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // -----------------------------------------------------------------------------------------------------------------
    // CRUD Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<T[]> GetAllAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        return ItemsById.Values.ToArray();
    }

    public async ValueTask<T?> GetByIdAsync(Guid id, CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        return ItemsById.GetValueOrDefault(id);
    }
    
    public async ValueTask<bool> SaveAsync(IEnumerable<T> items, CancellationToken ct = default) {
        if (!Storage.IsLocalhost) return false;
        if (!await Storage.VerifyPermissionAsync(ct)) return false;

        string json = await AsJsonStringAsync(items, ct);
        bool success = await Storage.WriteIndexAsync(json, ct);
        return success;
    }

    public async ValueTask<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default) {
        _ = ct;
        if (!Storage.IsLocalhost || !await Storage.HasAccessAsync(ct)) return false;
        if (!await Storage.VerifyPermissionAsync(ct)) return false;

        await EnsureCacheAsync(ct);
        if (!ItemsById.TryGetValue(id, out T? item)) return false;

        bool allDeleted = await Storage.DeleteLocalizedFilesAsync(item.MarkdownFileName, ct);
        if (!allDeleted) return false;

        T[] updatedItems = ItemsById.Remove(id).Values.ToArray();
        return await SaveAsync(updatedItems, ct);
    }

    public async ValueTask<string> GetAsJsonStringAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        return await AsJsonStringAsync(ItemsById.Values, ct);
    }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private async ValueTask<string> AsJsonStringAsync(IEnumerable<T> items, CancellationToken ct = default) {
        await using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, items, _jsonSerializerOptions, ct);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    protected async Task EnsureCacheAsync(CancellationToken ct) {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (_hasLoaded && !ShouldRefresh(now)) return;

        await _lock.WaitAsync(ct);
        try {
            now = DateTimeOffset.UtcNow;
            if (_hasLoaded && !ShouldRefresh(now)) return;

            await RefreshCacheAsync(now, ct);
        }
        catch {
            ItemsById = ImmutableDictionary<Guid, T>.Empty;
            _hasLoaded = true;
            _etag = null;
            _lastModifiedUtc = null;
            _lastRefreshUtc = now;
        }
        finally {
            _lock.Release();
        }
    }
    
    private bool ShouldRefresh(DateTimeOffset now)
        => !_hasLoaded || _lastRefreshUtc is null || now - _lastRefreshUtc.Value > GetRefreshWindow();

    private TimeSpan GetRefreshWindow()
        => Storage.IsLocalhost ? DevRefreshWindow : CacheRefreshWindow;

    private async Task RefreshCacheAsync(DateTimeOffset now, CancellationToken ct) {
        using HttpRequestMessage request = new(HttpMethod.Get, IndexPath);
        if (_etag is not null) request.Headers.IfNoneMatch.Add(_etag);
        else if (_lastModifiedUtc is not null) request.Headers.IfModifiedSince = _lastModifiedUtc;

        using HttpResponseMessage response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.StatusCode == HttpStatusCode.NotModified && _hasLoaded) {
            _lastRefreshUtc = now;
            if (response.Headers.ETag is not null) _etag = response.Headers.ETag;
            if (response.Content.Headers.LastModified is not null) _lastModifiedUtc = response.Content.Headers.LastModified;
            return;
        }

        response.EnsureSuccessStatusCode();
        T[]? items = await response.Content.ReadFromJsonAsync<T[]>(cancellationToken: ct);
        ItemsById = BuildIndex(items ?? []);
        _hasLoaded = true;
        _etag = response.Headers.ETag;
        _lastModifiedUtc = response.Content.Headers.LastModified;
        _lastRefreshUtc = now;
    }

    private static ImmutableDictionary<Guid, T> BuildIndex(IEnumerable<T> items) {
        ImmutableDictionary<Guid, T>.Builder builder = ImmutableDictionary.CreateBuilder<Guid, T>();
        foreach (T item in items) {
            builder[item.Id] = item;
        }

        return builder.ToImmutable();
    }
}
