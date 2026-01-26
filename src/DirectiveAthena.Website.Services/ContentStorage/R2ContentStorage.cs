// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using DirectiveAthena.Website.Services.Localization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace DirectiveAthena.Website.Services.ContentStorage;
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
    ILogger logger
) : IContentStorage {
    private readonly bool _canWrite = options.CanWrite;
    private readonly Uri? _proxyUploadEndpoint = BuildProxyEndpoint(options.ProxyEndpoint, "upload");
    private readonly Uri? _proxyDeleteEndpoint = BuildProxyEndpoint(options.ProxyEndpoint, "delete");
    private readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string IndexContentPath => BuildPublicUrl(GetIndexDiskPath());

    // -----------------------------------------------------------------------------------------------------------------
    // File Access
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<ContentReadResult> ReadIndexAsync(EntityTagHeaderValue? etag, DateTimeOffset? lastModifiedUtc, CancellationToken ct = default) {
        using HttpRequestMessage request = new(HttpMethod.Get, IndexContentPath);
        if (etag is not null) request.Headers.IfNoneMatch.Add(etag);
        else if (lastModifiedUtc is not null) request.Headers.IfModifiedSince = lastModifiedUtc;

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        string? content = response.StatusCode == HttpStatusCode.NotModified
            ? null
            : await response.Content.ReadAsStringAsync(ct);

        return new ContentReadResult(
            response.StatusCode,
            content,
            response.Headers.ETag,
            response.Content.Headers.LastModified
        );
    }

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
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
            return true;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to write R2 object {Key}.", key);
            return false;
        }
    }

    private async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default) {
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
            var deleteArgs = new RemoveObjectArgs()
                .WithBucket(options.BucketName!)
                .WithObject(key);

            await minioClient.RemoveObjectAsync(deleteArgs, ct);
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
            return response.IsSuccessStatusCode;
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
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to delete R2 object {Key} via proxy.", key);
            return false;
        }
    }

    [UsedImplicitly] private record ProxyUploadRequest(string Key, string Content, string ContentType);
    [UsedImplicitly] private record ProxyDeleteRequest(string Key);

    private static Uri? BuildProxyEndpoint(string? proxyEndpoint, string operation) {
        if (string.IsNullOrWhiteSpace(proxyEndpoint)) return null;

        string baseUrl = proxyEndpoint.Trim().TrimEnd('/');
        return new Uri($"{baseUrl}/{operation}", UriKind.Absolute);

    }
}
