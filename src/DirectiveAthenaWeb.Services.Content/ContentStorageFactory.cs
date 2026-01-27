// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;

namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContentStorageFactory>]
public class ContentStorageFactory(
    ILocalizationProvider localizationProvider,
    IOptions<R2StorageOptions> r2Options,
    HttpClient httpClient,
    ILoggerFactory loggerFactory
) : IContentStorageFactory {
    private const string ContentRoot = "content";
    private static bool _browserWarningLogged;

    public IContentStorage ForCategory(ContentCategory category) {
        string folder = GetCategoryFolder(category);
        ILogger r2Logger = loggerFactory.CreateLogger<R2ContentStorage>();
        r2Logger.Debug("Creating R2 content storage for {Category} at {Folder}.", category, folder);

        return new R2ContentStorage(localizationProvider,
            r2Options.Value,
            folder,
            BuildPublicBaseUri(r2Options.Value, r2Logger),
            httpClient,
            CreateMinioClient(r2Options.Value, r2Logger),
            r2Logger
        );
    }

    private static string GetCategoryFolder(ContentCategory category)
        => category switch {
            ContentCategory.Writings => $"{ContentRoot}/writings",
            ContentCategory.WorldFaq => $"{ContentRoot}/world-faq",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, @"Unsupported content category.")
        };

    private static Uri BuildPublicBaseUri(R2StorageOptions options, ILogger logger) {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl)) {
            logger.Warning("R2 public base URL is missing; content reads may fail.");
            return new Uri("https://localhost/");
        }

        string baseUrl = options.PublicBaseUrl.Trim();
        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private static IMinioClient? CreateMinioClient(R2StorageOptions options, ILogger logger) {
        if (OperatingSystem.IsBrowser()) {
            if (!_browserWarningLogged) {
                logger.Debug("R2 client is not supported in browser contexts; proxy uploads must be used for writes.");
                _browserWarningLogged = true;
            }

            return null;
        }

        if (!string.IsNullOrWhiteSpace(options.ProxyEndpoint)) {
            logger.Information("R2 proxy endpoint configured; using proxy uploads instead of direct S3 client.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.AccountId) || string.IsNullOrWhiteSpace(options.BucketName)) {
            logger.Warning("R2 account or bucket is missing; content reads and writes may fail.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.AccessKeyId) || string.IsNullOrWhiteSpace(options.SecretAccessKey)) {
            logger.Warning("R2 credentials are missing; MinIO client requires access keys for writes.");
            return null;
        }

        string endpointHost = $"{options.AccountId}.r2.cloudflarestorage.com";
        IMinioClient client = new MinioClient()
            .WithEndpoint(endpointHost)
            .WithCredentials(options.AccessKeyId, options.SecretAccessKey)
            .WithSSL()
            .Build();

        logger.Debug("Created R2 MinIO client for {AccountId}.", options.AccountId);
        return client;
    }
}
