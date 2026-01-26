// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2ContentStorage(
    HttpClient http,
    ILocalFileStorage localFileStorage,
    ILocalizationProvider localizationProvider,
    IOptions<R2StorageOptions> options,
    string categoryFolder,
    ILogger logger
) : IContentStorage {
    private readonly R2StorageOptions _options = options.Value;
    private readonly Uri _publicBaseUri = BuildPublicBaseUri(options.Value, logger);
    private readonly Uri _bucketEndpoint = BuildBucketEndpoint(options.Value, logger);

    public bool IsLocalhost => localFileStorage.IsLocalhost;
    public bool IsWritable => _options.IsWriteConfigured;
    public bool RequiresLocalFileSystemAccess => false;
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

        string key = NormalizeKey(relativePath);
        byte[] payload = Encoding.UTF8.GetBytes(content);
        using HttpRequestMessage request = new(HttpMethod.Put, BuildBucketUrl(key));
        request.Content = new ByteArrayContent(payload);
        request.Content.Headers.ContentType = new(ResolveContentType(key));
        SignRequest(request, payload);

        using HttpResponseMessage response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) {
            logger.Warning("Failed to write R2 object {Key}: {StatusCode}.", key, response.StatusCode);
        }

        return response.IsSuccessStatusCode;
    }

    public async ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default) {
        string key = NormalizeKey(relativePath);
        using HttpResponseMessage response = await http.GetAsync(BuildPublicUrl(key), ct);
        if (response.StatusCode == HttpStatusCode.NotFound) {
            return null;
        }

        if (!response.IsSuccessStatusCode) {
            logger.Warning("Failed to read R2 object {Key}: {StatusCode}.", key, response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsStringAsync(ct);
    }

    public async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default) {
        if (!IsWritable) {
            logger.Warning("Skipping delete for {Path} because R2 writes are not enabled.", relativePath);
            return false;
        }

        string key = NormalizeKey(relativePath);
        using HttpRequestMessage request = new(HttpMethod.Delete, BuildBucketUrl(key));
        SignRequest(request, Array.Empty<byte>());

        using HttpResponseMessage response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) {
            return true;
        }

        if (!response.IsSuccessStatusCode) {
            logger.Warning("Failed to delete R2 object {Key}: {StatusCode}.", key, response.StatusCode);
        }

        return response.IsSuccessStatusCode;
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

    // -----------------------------------------------------------------------------------------------------------------
    // Signing Helpers
    // -----------------------------------------------------------------------------------------------------------------
    private void SignRequest(HttpRequestMessage request, byte[] payload) {
        string region = string.IsNullOrWhiteSpace(_options.Region) ? "auto" : _options.Region;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string amzDate = now.ToString("yyyyMMddTHHmmssZ");
        string dateStamp = now.ToString("yyyyMMdd");
        string payloadHash = ToHexHash(payload);

        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

        string canonicalUri = GetCanonicalUri(request.RequestUri);
        string canonicalHeaders = $"host:{request.RequestUri!.Host}\n" +
                                  $"x-amz-content-sha256:{payloadHash}\n" +
                                  $"x-amz-date:{amzDate}\n";
        const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
        string canonicalRequest = $"{request.Method}\n{canonicalUri}\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
        string canonicalRequestHash = ToHexHash(Encoding.UTF8.GetBytes(canonicalRequest));
        string credentialScope = $"{dateStamp}/{region}/s3/aws4_request";
        string stringToSign = $"AWS4-HMAC-SHA256\n{amzDate}\n{credentialScope}\n{canonicalRequestHash}";

        byte[] signingKey = GetSignatureKey(_options.SecretAccessKey!, dateStamp, region, "s3");
        string signature = ToHexString(HmacSha256(signingKey, stringToSign));
        string authorization = $"AWS4-HMAC-SHA256 Credential={_options.AccessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

        request.Headers.TryAddWithoutValidation("Authorization", authorization);
    }

    private static string GetCanonicalUri(Uri? requestUri) {
        string path = requestUri?.AbsolutePath ?? "/";
        if (string.IsNullOrEmpty(path)) return "/";

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string canonical = "/" + string.Join("/", segments.Select(Uri.EscapeDataString));
        return canonical;
    }

    private static byte[] HmacSha256(byte[] key, string data) {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static byte[] GetSignatureKey(string secretKey, string dateStamp, string region, string service) {
        byte[] kDate = HmacSha256(Encoding.UTF8.GetBytes($"AWS4{secretKey}"), dateStamp);
        byte[] kRegion = HmacSha256(kDate, region);
        byte[] kService = HmacSha256(kRegion, service);
        return HmacSha256(kService, "aws4_request");
    }

    private static string ToHexHash(byte[] data) {
        byte[] hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ToHexString(byte[] data)
        => Convert.ToHexString(data).ToLowerInvariant();

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

    private Uri BuildBucketUrl(string relativePath) {
        string key = NormalizeKey(relativePath);
        return new Uri(_bucketEndpoint, key);
    }

    private static Uri BuildPublicBaseUri(R2StorageOptions options, ILogger logger) {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl)) {
            logger.Warning("R2 public base URL is missing; content reads may fail.");
            return new Uri("https://localhost/");
        }

        string baseUrl = options.PublicBaseUrl.Trim();
        if (!baseUrl.EndsWith("/")) {
            baseUrl += "/";
        }

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static Uri BuildBucketEndpoint(R2StorageOptions options, ILogger logger) {
        if (string.IsNullOrWhiteSpace(options.AccountId) || string.IsNullOrWhiteSpace(options.BucketName)) {
            logger.Warning("R2 account or bucket is missing; writes will fail.");
            return new Uri("https://localhost/");
        }

        return new Uri($"https://{options.AccountId}.r2.cloudflarestorage.com/{options.BucketName}/", UriKind.Absolute);
    }
}
