// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2ContentStorage(
    ILocalizationProvider localizationProvider,
    R2StorageOptions options,
    string categoryFolder,
    Uri publicBaseUri,
    HttpClient httpClient,
    IMinioClient? minioClient,
    ILogger logger,
    bool allowInMemoryFallback,
    IR2StatusTracker? statusTracker = null
) : IContentStorage {
    private readonly bool _canWrite = options.CanWrite;
    private readonly Uri? _proxyUploadEndpoint = BuildProxyEndpoint(options.ProxyEndpoint, "upload");
    private readonly Uri? _proxyDeleteEndpoint = BuildProxyEndpoint(options.ProxyEndpoint, "delete");
    private readonly ConcurrentDictionary<string, InMemoryFile> _inMemoryFiles = new(StringComparer.Ordinal);
    private volatile bool _useInMemoryFallback;
    private readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string IndexContentPath => BuildPublicUrl(GetIndexDiskPath());

    // -----------------------------------------------------------------------------------------------------------------
    // File Access
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<ContentReadResult> ReadIndexAsync(EntityTagHeaderValue? etag, DateTimeOffset? lastModifiedUtc, CancellationToken ct = default) {
        if (_useInMemoryFallback) {
            return ReadIndexFromMemory(etag, lastModifiedUtc);
        }

        using HttpRequestMessage request = new(HttpMethod.Get, IndexContentPath);
        if (etag is not null) request.Headers.IfNoneMatch.Add(etag);
        else if (lastModifiedUtc is not null) request.Headers.IfModifiedSince = lastModifiedUtc;

        try {
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            string? content = response.StatusCode == HttpStatusCode.NotModified
                ? null
                : await response.Content.ReadAsStringAsync(ct);

            if (allowInMemoryFallback && response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(content)) {
                StoreInMemory(GetIndexDiskPath(), content, response.Headers.ETag, response.Content.Headers.LastModified);
            }

            return new ContentReadResult(
                response.StatusCode,
                content,
                response.Headers.ETag,
                response.Content.Headers.LastModified
            );
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            return ReadIndexFromMemory(etag, lastModifiedUtc);
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct)) {
            logger.Error(ex, "Failed to reach R2 while reading {Path}.", IndexContentPath);
            return new ContentReadResult(HttpStatusCode.ServiceUnavailable, null, null, null);
        }
    }

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
        if (_useInMemoryFallback) {
            StoreInMemory(relativePath, content);
            return true;
        }

        if (!_canWrite) {
            logger.Warning("Skipping write for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        if (minioClient is null) {
            if (_proxyUploadEndpoint is not null) {
                return await WriteFileWithProxyAsync(key, content, ct);
            }

            logger.Warning("Skipping write for {Path} because proxy endpoint is not configured.", key);
            return false;
        }

        try {
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            using var contentStream = new MemoryStream(contentBytes);
            var putArgs = new PutObjectArgs()
                .WithBucket(options.BucketName!)
                .WithObject(key)
                .WithStreamData(contentStream)
                .WithObjectSize(contentBytes.Length)
                .WithContentType(ResolveContentType(key));

            await minioClient.PutObjectAsync(putArgs, ct);
            if (allowInMemoryFallback) {
                StoreInMemory(relativePath, content);
            }
            return true;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            StoreInMemory(relativePath, content);
            return true;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to write R2 object {Key}.", key);
            return false;
        }
    }

    private async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default) {
        if (_useInMemoryFallback) {
            RemoveFromMemory(relativePath);
            return true;
        }

        if (!_canWrite) {
            logger.Warning("Skipping delete for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        if (minioClient is null) {
            if (_proxyDeleteEndpoint is not null) {
                return await DeleteFileWithProxyAsync(key, ct);
            }

            logger.Warning("Skipping delete for {Path} because proxy endpoint is not configured.", key);
            return false;
        }

        try {
            RemoveObjectArgs? deleteArgs = new RemoveObjectArgs()
                .WithBucket(options.BucketName!)
                .WithObject(key);

            await minioClient.RemoveObjectAsync(deleteArgs, ct);
            if (allowInMemoryFallback) {
                RemoveFromMemory(relativePath);
            }
            return true;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            RemoveFromMemory(relativePath);
            return true;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to delete R2 object {Key}.", key);
            return false;
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Path Helpers
    // -----------------------------------------------------------------------------------------------------------------
    public string GetMarkdownContentPath(string locale, string fileName)
        => BuildPublicUrl($"{categoryFolder}/{locale}/{fileName}");

    private string GetIndexDiskPath()
        => $"{categoryFolder}/index.json";

    public string GetMarkdownDiskPath(string locale, string fileName)
        => $"{categoryFolder}/{locale}/{fileName}";

    // -----------------------------------------------------------------------------------------------------------------
    // Content Ops
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default)
        => WriteFileAsync(GetIndexDiskPath(), content, ct);

    public async ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default) {
        if (_useInMemoryFallback) {
            return ReadFromMemory(relativePath);
        }

        string path = BuildPublicUrl(relativePath);
        try {
            using HttpResponseMessage response = await httpClient.GetAsync(path, ct);
            if (!response.IsSuccessStatusCode) return null;

            string content = await response.Content.ReadAsStringAsync(ct);
            if (allowInMemoryFallback && !string.IsNullOrWhiteSpace(content)) {
                StoreInMemory(relativePath, content);
            }

            return content;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            return ReadFromMemory(relativePath);
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct)) {
            logger.Error(ex, "Failed to reach R2 while reading {Path}.", path);
            return null;
        }
    }

    public async Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default) {
        Task<bool>[] deleteTasks = localizationProvider.GetSupportedLocalizations()
            .Select(culture => DeleteFileAsync(GetMarkdownDiskPath(culture.Code, fileName), ct).AsTask())
            .ToArray();

        bool[] results = await Task.WhenAll(deleteTasks);
        bool success = results.All(result => result);
        logger.Information("Delete localized R2 files for {FileName} {Result}.", fileName, success ? "succeeded" : "failed");
        return success;
    }

    // ReSharper disable once ConvertIfStatementToReturnStatement
    private static string ResolveContentType(string key) {
        if (key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return "application/json";
        if (key.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) return "text/markdown";

        return "text/plain";
    }

    private static string NormalizeKey(string relativePath)
        => relativePath.Trim().TrimStart('/');

    private string BuildPublicUrl(string relativePath) {
        string key = NormalizeKey(relativePath);
        return new Uri(publicBaseUri, key).ToString();
    }

    private async ValueTask<bool> WriteFileWithProxyAsync(string key, string content, CancellationToken ct) {
        var payload = new ProxyUploadRequest(key, content, ResolveContentType(key));
        using var request = new HttpRequestMessage(HttpMethod.Post, _proxyUploadEndpoint);
        request.Content = JsonContent.Create(payload, options: _jsonOptions);

        try {
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
            if (allowInMemoryFallback && response.IsSuccessStatusCode) {
                StoreInMemory(key, content);
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            StoreInMemory(key, content);
            return true;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to upload R2 object {Key} via proxy.", key);
            return false;
        }
    }

    private async ValueTask<bool> DeleteFileWithProxyAsync(string key, CancellationToken ct) {
        var payload = new ProxyDeleteRequest(key);
        using var request = new HttpRequestMessage(HttpMethod.Post, _proxyDeleteEndpoint);
        request.Content = JsonContent.Create(payload, options: _jsonOptions);

        try {
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
            if (allowInMemoryFallback && response.IsSuccessStatusCode) {
                RemoveFromMemory(key);
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (IsConnectionFailure(ex, ct) && allowInMemoryFallback) {
            ActivateInMemoryFallback(ex);
            RemoveFromMemory(key);
            return true;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to delete R2 object {Key} via proxy.", key);
            return false;
        }
    }

    private static Uri? BuildProxyEndpoint(string? proxyEndpoint, string operation) {
        if (string.IsNullOrWhiteSpace(proxyEndpoint)) return null;

        string baseUrl = proxyEndpoint.Trim().TrimEnd('/');
        return new Uri($"{baseUrl}/{operation}", UriKind.Absolute);

    }

    private void ActivateInMemoryFallback(Exception? exception) {
        if (_useInMemoryFallback) return;

        _useInMemoryFallback = true;
        statusTracker?.ActivateFallback(exception?.Message);
        if (exception is null) {
            logger.Information("R2 fallback activated for {Folder}; using in-memory content store.", categoryFolder);
            return;
        }

        logger.Information(exception, "R2 unreachable for {Folder}; using in-memory content store.", categoryFolder);
    }

    private static bool IsConnectionFailure(Exception ex, CancellationToken ct)
        => ex is HttpRequestException
           || (ex is TaskCanceledException && !ct.IsCancellationRequested);

    private ContentReadResult ReadIndexFromMemory(EntityTagHeaderValue? etag, DateTimeOffset? lastModifiedUtc) {
        string key = NormalizeKey(GetIndexDiskPath());
        if (!_inMemoryFiles.TryGetValue(key, out InMemoryFile? file)) {
            return new ContentReadResult(HttpStatusCode.NotFound, null, null, null);
        }

        bool isNotModified = false;
        if (etag is not null && file.ETag is not null) {
            isNotModified = string.Equals(etag.Tag, file.ETag.Tag, StringComparison.Ordinal);
        }
        else if (lastModifiedUtc is not null) {
            isNotModified = file.LastModifiedUtc <= lastModifiedUtc.Value;
        }

        return isNotModified
            ? new ContentReadResult(HttpStatusCode.NotModified, null, file.ETag, file.LastModifiedUtc)
            : new ContentReadResult(HttpStatusCode.OK, file.Content, file.ETag, file.LastModifiedUtc);
    }

    private string? ReadFromMemory(string relativePath) {
        string key = NormalizeKey(relativePath);
        return _inMemoryFiles.TryGetValue(key, out InMemoryFile? file) ? file.Content : null;
    }

    private void StoreInMemory(string relativePath, string content, EntityTagHeaderValue? etag = null, DateTimeOffset? lastModifiedUtc = null) {
        string key = NormalizeKey(relativePath);
        DateTimeOffset lastModified = lastModifiedUtc ?? DateTimeOffset.UtcNow;
        EntityTagHeaderValue resolvedEtag = etag ?? CreateETag(content);
        _inMemoryFiles[key] = new InMemoryFile(content, resolvedEtag, lastModified);
    }

    private void RemoveFromMemory(string relativePath) {
        string key = NormalizeKey(relativePath);
        _inMemoryFiles.TryRemove(key, out _);
    }

    private static EntityTagHeaderValue CreateETag(string content) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        string hash = Convert.ToHexString(SHA256.HashData(bytes));
        return new EntityTagHeaderValue($"\"{hash}\"");
    }

    private sealed record InMemoryFile(string Content, EntityTagHeaderValue? ETag, DateTimeOffset LastModifiedUtc);
}
