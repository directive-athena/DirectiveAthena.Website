// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.ContentStorage;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentRepository<T>(IContentStorage contentStorage, ILogger logger) : IContentRepository<T>
    where T : ContentBase {
    protected ILogger Logger { get; } = logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    protected IContentStorage Storage { get; } = contentStorage;
    protected ImmutableDictionary<Guid, T> ItemsById { get; private set; } = ImmutableDictionary<Guid, T>.Empty;
    private bool _hasLoaded;
    private EntityTagHeaderValue? _etag;
    private DateTimeOffset? _lastModifiedUtc;
    private DateTimeOffset? _lastRefreshUtc;
    private readonly TimeSpan DevRefreshWindow = TimeSpan.FromSeconds(5);

    protected abstract string IndexPath { get; }

    private readonly JsonSerializerOptions _jsonReadOptions = new(JsonSerializerDefaults.Web);
    private readonly JsonSerializerOptions _jsonWriteOptions = new() {
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
        ICollection<T> itemList = items as ICollection<T> ?? items.ToArray();
        Logger.Information("Saving {ContentType} index with {Count} items.", typeof(T).Name, itemList.Count);
        string json = await AsJsonStringAsync(itemList, ct);
        bool success = await Storage.WriteIndexAsync(json, ct);
        Logger.Information("Save {ContentType} index {Result}.", typeof(T).Name, success ? "succeeded" : "failed");
        return success;
    }

    public async ValueTask<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        if (!ItemsById.TryGetValue(id, out T? item)) {
            Logger.Warning("{ContentType} {Id} not found for deletion.", typeof(T).Name, id);
            return false;
        }

        bool allDeleted = await Storage.DeleteLocalizedFilesAsync(item.MarkdownFileName, ct);
        if (!allDeleted) {
            Logger.Warning("Failed to delete localized files for {ContentType} {Id}.", typeof(T).Name, id);
            return false;
        }

        T[] updatedItems = ItemsById.Remove(id).Values.ToArray();
        Logger.Information("Deleted {ContentType} {Id}, saving updated index.", typeof(T).Name, id);
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
        await JsonSerializer.SerializeAsync(stream, items, _jsonWriteOptions, ct);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    protected async Task EnsureCacheAsync(CancellationToken ct) {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (_hasLoaded && !ShouldRefresh(now)) {
            Logger.Debug("{ContentType} cache is still fresh; skipping refresh.", typeof(T).Name);
            return;
        }

        await _lock.WaitAsync(ct);
        try {
            now = DateTimeOffset.UtcNow;
            if (_hasLoaded && !ShouldRefresh(now)) {
                Logger.Debug("{ContentType} cache was refreshed by another caller.", typeof(T).Name);
                return;
            }

            await RefreshCacheAsync(now, ct);
        }
        catch (Exception ex) {
            Logger.LogError(ex, "Failed to refresh {ContentType} cache; clearing cached data.", typeof(T).Name);
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
        #if DEBUG
        => DevRefreshWindow;
    #else
        => CacheRefreshWindow;
    #endif

    private async Task RefreshCacheAsync(DateTimeOffset now, CancellationToken ct) {
        Logger.Debug("Refreshing {ContentType} cache from {Path}.", typeof(T).Name, IndexPath);
        ContentReadResult response = await Storage.ReadIndexAsync(_etag, _lastModifiedUtc, ct);
        switch (response.StatusCode) {
            case HttpStatusCode.NotFound:
                Logger.Warning("{ContentType} index not found at {Path}; treating as empty dataset.", typeof(T).Name, IndexPath);
                ItemsById = ImmutableDictionary<Guid, T>.Empty;
                _hasLoaded = true;
                _etag = null;
                _lastModifiedUtc = null;
                _lastRefreshUtc = now;
                return;

            case HttpStatusCode.NotModified when _hasLoaded: {
                _lastRefreshUtc = now;
                if (response.ETag is not null) _etag = response.ETag;
                if (response.LastModifiedUtc is not null) _lastModifiedUtc = response.LastModifiedUtc;
                Logger.Debug("{ContentType} cache not modified; updated refresh markers.", typeof(T).Name);
                return;
            }

            case HttpStatusCode.Continue:
            case HttpStatusCode.SwitchingProtocols:
            case HttpStatusCode.Processing:
            case HttpStatusCode.EarlyHints:
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
            case HttpStatusCode.Accepted:
            case HttpStatusCode.NonAuthoritativeInformation:
            case HttpStatusCode.NoContent:
            case HttpStatusCode.ResetContent:
            case HttpStatusCode.PartialContent:
            case HttpStatusCode.MultiStatus:
            case HttpStatusCode.AlreadyReported:
            case HttpStatusCode.IMUsed:
            case HttpStatusCode.Ambiguous:
            case HttpStatusCode.Moved:
            case HttpStatusCode.Found:
            case HttpStatusCode.RedirectMethod:
            case HttpStatusCode.UseProxy:
            case HttpStatusCode.Unused:
            case HttpStatusCode.RedirectKeepVerb:
            case HttpStatusCode.PermanentRedirect:
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.PaymentRequired:
            case HttpStatusCode.Forbidden:
            case HttpStatusCode.MethodNotAllowed:
            case HttpStatusCode.NotAcceptable:
            case HttpStatusCode.ProxyAuthenticationRequired:
            case HttpStatusCode.RequestTimeout:
            case HttpStatusCode.Conflict:
            case HttpStatusCode.Gone:
            case HttpStatusCode.LengthRequired:
            case HttpStatusCode.PreconditionFailed:
            case HttpStatusCode.RequestEntityTooLarge:
            case HttpStatusCode.RequestUriTooLong:
            case HttpStatusCode.UnsupportedMediaType:
            case HttpStatusCode.RequestedRangeNotSatisfiable:
            case HttpStatusCode.ExpectationFailed:
            case HttpStatusCode.MisdirectedRequest:
            case HttpStatusCode.UnprocessableEntity:
            case HttpStatusCode.Locked:
            case HttpStatusCode.FailedDependency:
            case HttpStatusCode.UpgradeRequired:
            case HttpStatusCode.PreconditionRequired:
            case HttpStatusCode.TooManyRequests:
            case HttpStatusCode.RequestHeaderFieldsTooLarge:
            case HttpStatusCode.UnavailableForLegalReasons:
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.NotImplemented:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
            case HttpStatusCode.HttpVersionNotSupported:
            case HttpStatusCode.VariantAlsoNegotiates:
            case HttpStatusCode.InsufficientStorage:
            case HttpStatusCode.LoopDetected:
            case HttpStatusCode.NotExtended:
            case HttpStatusCode.NetworkAuthenticationRequired: break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"Failed to load {typeof(T).Name} index: {response.StatusCode}.");
        }

        T[]? items = string.IsNullOrWhiteSpace(response.Content)
            ? []
            : JsonSerializer.Deserialize<T[]>(response.Content, _jsonReadOptions);
        ItemsById = BuildIndex(items ?? []);
        _hasLoaded = true;
        _etag = response.ETag;
        _lastModifiedUtc = response.LastModifiedUtc;
        _lastRefreshUtc = now;
        Logger.Information("Loaded {Count} {ContentType} items into cache.", ItemsById.Count, typeof(T).Name);
    }

    private static ImmutableDictionary<Guid, T> BuildIndex(IEnumerable<T> items) {
        ImmutableDictionary<Guid, T>.Builder builder = ImmutableDictionary.CreateBuilder<Guid, T>();
        foreach (T item in items) {
            builder[item.Id] = item;
        }

        return builder.ToImmutable();
    }
}
