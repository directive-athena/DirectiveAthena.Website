// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.FileSystem;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class CachedJsonRepository<T>(
    HttpClient http,
    IDevFileSystemManager devFs
) {
    private readonly SemaphoreSlim _lock = new(1, 1);
    private ImmutableArray<T> _items;
    private bool _hasLoaded;
    private EntityTagHeaderValue? _etag;
    private DateTimeOffset? _lastModifiedUtc;
    private DateTimeOffset? _lastRefreshUtc;

    private readonly TimeSpan CacheRefreshWindow = TimeSpan.FromMinutes(5);
    private readonly TimeSpan DevRefreshWindow = TimeSpan.FromSeconds(5);

    protected abstract string IndexPath { get; }
    protected abstract string WritePath { get; }
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private string Serialize(IEnumerable<T> items)
        => JsonSerializer.Serialize(items, _jsonSerializerOptions);
    
    protected async ValueTask<ImmutableArray<T>> GetAllAsync(CancellationToken ct = default) {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (_hasLoaded && !ShouldRefresh(now)) return _items;

        await _lock.WaitAsync(ct);
        try {
            now = DateTimeOffset.UtcNow;
            if (_hasLoaded && !ShouldRefresh(now)) return _items;

            await RefreshCacheAsync(now, ct);
        }
        catch {
            _items = [];
            _hasLoaded = true;
            _etag = null;
            _lastModifiedUtc = null;
            _lastRefreshUtc = now;
        }
        finally {
            _lock.Release();
        }

        return _items;
    }

    protected void UpdateCache(IEnumerable<T> items) {
        _items = [..items];
        _hasLoaded = true;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        _etag = null;
        _lastModifiedUtc = null;
    }

    public virtual async ValueTask<bool> SaveAsync(IEnumerable<T> items, CancellationToken ct = default) {
        _ = ct;
        if (!devFs.IsLocalhost) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        T[] itemArray = items as T[] ?? items.ToArray();
        string json = Serialize(itemArray);
        bool success = await devFs.WriteFileAsync(WritePath, json);
        if (success) UpdateCache(itemArray);
        return success;
    }

    private bool ShouldRefresh(DateTimeOffset now)
        => !_hasLoaded || _lastRefreshUtc is null || now - _lastRefreshUtc.Value > GetRefreshWindow();

    private TimeSpan GetRefreshWindow()
        => devFs.IsLocalhost ? DevRefreshWindow : CacheRefreshWindow;

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
        _items = [..items ?? []];
        _hasLoaded = true;
        _etag = response.Headers.ETag;
        _lastModifiedUtc = response.Content.Headers.LastModified;
        _lastRefreshUtc = now;
    }
}
