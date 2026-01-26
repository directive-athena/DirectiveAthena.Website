// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
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
    IAmazonS3? s3Client,
    ILogger logger
) : IContentStorage {
    private readonly bool _canWrite = options.IsWriteConfigured;

    public string IndexContentPath => BuildPublicUrl(GetIndexDiskPath());

    // -----------------------------------------------------------------------------------------------------------------
    // File Access
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
        if (!_canWrite) {
            logger.Warning("Skipping write for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        if (s3Client is null) {
            logger.Warning("Skipping write for {Path} because R2 client is not configured.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
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

        if (s3Client is null) {
            logger.Warning("Skipping delete for {Path} because R2 client is not configured.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
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
}
