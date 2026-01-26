// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2ContentStorage(
    ILocalizationProvider localizationProvider,
    IOptions<R2StorageOptions> options,
    string categoryFolder,
    ILogger logger
) : IContentStorage {
    private readonly R2StorageOptions _options = options.Value;
    private readonly Uri _publicBaseUri = BuildPublicBaseUri(options.Value, logger);
    private readonly IAmazonS3? _s3Client = CreateS3Client(options.Value, logger);

    public bool IsWritable => _options.IsWriteConfigured;
    public string IndexContentPath => BuildPublicUrl(GetIndexDiskPath());

    // -----------------------------------------------------------------------------------------------------------------
    // File Access
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> HasAccessAsync(CancellationToken ct = default)
        => new(IsWritable);

    public ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default)
        => new(IsWritable);

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
        if (!IsWritable) {
            logger.Warning("Skipping write for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        if (_s3Client is null) {
            logger.Warning("Skipping write for {Path} because R2 client is not configured.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        PutObjectRequest request = new() {
            BucketName = _options.BucketName!,
            Key = key,
            ContentBody = content,
            ContentType = ResolveContentType(key)
        };

        try {
            PutObjectResponse response = await _s3Client.PutObjectAsync(request, ct);
            return response.HttpStatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to write R2 object {Key}.", key);
            return false;
        }
    }

    public async ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default) {
        if (_s3Client is null) {
            logger.Warning("Skipping read for {Path} because R2 client is not configured.", relativePath);
            return null;
        }

        string key = NormalizeKey(relativePath);
        GetObjectRequest request = new() {
            BucketName = _options.BucketName!,
            Key = key
        };

        try {
            using GetObjectResponse response = await _s3Client.GetObjectAsync(request, ct);
            using StreamReader reader = new(response.ResponseStream);
            return await reader.ReadToEndAsync(ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
            return null;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to read R2 object {Key}.", key);
            return null;
        }
    }

    public async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default) {
        if (!IsWritable) {
            logger.Warning("Skipping delete for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        if (_s3Client is null) {
            logger.Warning("Skipping delete for {Path} because R2 client is not configured.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        DeleteObjectRequest request = new() {
            BucketName = _options.BucketName!,
            Key = key
        };

        try {
            DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(request, ct);
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

    public string GetIndexDiskPath()
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

    private static string ResolveContentType(string key) {
        if (key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return "application/json";
        if (key.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) return "text/markdown";

        return "text/plain";
    }

    private static string NormalizeKey(string relativePath)
        => relativePath.Trim().TrimStart('/');

    private string BuildPublicUrl(string relativePath) {
        string key = NormalizeKey(relativePath);
        return new Uri(_publicBaseUri, key).ToString();
    }

    private static Uri BuildPublicBaseUri(R2StorageOptions options, ILogger logger) {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl)) {
            logger.Warning("R2 public base URL is missing; content reads may fail.");
            return new Uri("https://localhost/");
        }

        string baseUrl = options.PublicBaseUrl.Trim();
        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static IAmazonS3? CreateS3Client(R2StorageOptions options, ILogger logger) {
        if (string.IsNullOrWhiteSpace(options.AccountId) || string.IsNullOrWhiteSpace(options.BucketName)) {
            logger.Warning("R2 account or bucket is missing; content reads and writes may fail.");
            return null;
        }

        string regionName = string.IsNullOrWhiteSpace(options.Region) || options.Region.Equals("auto", StringComparison.OrdinalIgnoreCase)
            ? "us-east-1"
            : options.Region;

        AmazonS3Config config = new() {
            ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            RegionEndpoint = RegionEndpoint.GetBySystemName(regionName),
            AuthenticationRegion = regionName
        };

        AWSCredentials credentials;
        if (options.IsWriteConfigured) {
            credentials = new BasicAWSCredentials(options.AccessKeyId!, options.SecretAccessKey!);
        }
        else {
            credentials = new AnonymousAWSCredentials();
            logger.Warning("R2 credentials are missing; using anonymous client for reads only.");
        }

        logger.Debug("Created R2 S3 client for {AccountId}.", options.AccountId);
        return new AmazonS3Client(credentials, config);
    }
}
