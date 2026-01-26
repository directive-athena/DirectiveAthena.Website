// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;

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
    IAmazonS3? s3Client,
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
    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
        if (!_canWrite) {
            logger.Warning("Skipping write for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        if (s3Client is null) {
            if (_proxyUploadEndpoint is not null) {
                return await WriteFileWithProxyAsync(key, content, ct);
            }

            logger.Warning("Skipping write for {Path} because proxy endpoint is not configured.", key);
            return false;
        }

        PutObjectRequest request = new() {
            BucketName = options.BucketName!,
            Key = key,
            ContentBody = content,
            ContentType = ResolveContentType(key)
        };

        try {
            PutObjectResponse response = await s3Client.PutObjectAsync(request, ct);
            return response.HttpStatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
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
        if (s3Client is null) {
            if (_proxyDeleteEndpoint is not null) {
                return await DeleteFileWithProxyAsync(key, ct);
            }

            logger.Warning("Skipping delete for {Path} because proxy endpoint is not configured.", key);
            return false;
        }

        DeleteObjectRequest request = new() {
            BucketName = options.BucketName!,
            Key = key
        };

        try {
            DeleteObjectResponse response = await s3Client.DeleteObjectAsync(request, ct);
            return response.HttpStatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
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
        using var request = new HttpRequestMessage(HttpMethod.Post, _proxyUploadEndpoint) {
            Content = JsonContent.Create(payload, options: _jsonOptions)
        };

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
        using var request = new HttpRequestMessage(HttpMethod.Post, _proxyDeleteEndpoint) {
            Content = JsonContent.Create(payload, options: _jsonOptions)
        };

        try {
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to delete R2 object {Key} via proxy.", key);
            return false;
        }
    }

    private sealed record ProxyUploadRequest(string Key, string Content, string ContentType);
    private sealed record ProxyDeleteRequest(string Key);

    private static Uri? BuildProxyEndpoint(string? proxyEndpoint, string operation) {
        if (!string.IsNullOrWhiteSpace(proxyEndpoint)) {
            string baseUrl = proxyEndpoint.Trim().TrimEnd('/');
            return new Uri($"{baseUrl}/{operation}", UriKind.Absolute);
        }

        return null;
    }
}
