// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Content;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;

namespace DirectiveAthenaWeb.Services.R2Storage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IR2StorageFactory>]
public class R2ContentStorageFactory(
    ILocalizationProvider localizationProvider,
    IOptions<R2StorageOptions> r2Options,
    HttpClient httpClient,
    IHostEnvironment environment,
    ILoggerFactory loggerFactory,
    IR2StatusTracker? statusTracker = null
) : IR2StorageFactory {
    private const string ContentRoot = "content";
    private static bool _browserWarningLogged;
    internal static readonly Dictionary<Type, string> CategoryMap = new();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public IR2Storage<TContent> ForCategory<TContent>() where TContent : IContent {
        if (!CategoryMap.TryGetValue(typeof(TContent), out string? category)) throw new InvalidOperationException("No category mapping found.");
        
        string folder = Path.Combine(ContentRoot, category).Replace('\\', '/');
        ILogger r2Logger = loggerFactory.CreateLogger<R2ContentStorage<TContent>>();
        r2Logger.Debug("Creating R2 content storage for {Category} at {Folder}.", category, folder);

        return new R2ContentStorage<TContent>(localizationProvider,
            r2Options.Value,
            folder,
            BuildPublicBaseUri(r2Options.Value, r2Logger),
            httpClient,
            CreateMinioClient(r2Options.Value, r2Logger),
            r2Logger,
            environment.IsDevelopment(),
            statusTracker
        );
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

    private static IMinioClient? CreateMinioClient(R2StorageOptions options, ILogger logger) {
        if (OperatingSystem.IsBrowser()) {
            if (_browserWarningLogged) return null;

            logger.Debug("R2 client is not supported in browser contexts; proxy uploads must be used for writes.");
            _browserWarningLogged = true;

            return null;
        }

        if (!options.ProxyEndpoint.IsNullOrWhiteSpace()) {
            logger.Information("R2 proxy endpoint configured; using proxy uploads instead of direct S3 client.");
            return null;
        }

        if (options.AccountId.IsNullOrWhiteSpace() || options.BucketName.IsNullOrWhiteSpace()) {
            logger.Warning("R2 account or bucket is missing; content reads and writes may fail.");
            return null;
        }

        if (options.AccessKeyId.IsNullOrWhiteSpace() || options.SecretAccessKey.IsNullOrWhiteSpace()) {
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
