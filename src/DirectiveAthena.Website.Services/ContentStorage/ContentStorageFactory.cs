// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.ContentStorage;
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

    public IContentStorage ForCategory(ContentCategory category) {
        string folder = GetCategoryFolder(category);
        ILogger r2Logger = loggerFactory.CreateLogger<R2ContentStorage>();
        r2Logger.Debug("Creating R2 content storage for {Category} at {Folder}.", category, folder);
        
        return new R2ContentStorage(localizationProvider,
            r2Options.Value,
            folder,
            BuildPublicBaseUri(r2Options.Value, r2Logger),
            httpClient,
            CreateS3Client(r2Options.Value, r2Logger),
            r2Logger
        );
    }

    private static string GetCategoryFolder(ContentCategory category)
        => category switch {
            ContentCategory.Articles => $"{ContentRoot}/articles",
            ContentCategory.WorldRules => $"{ContentRoot}/world-rules",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported content category.")
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

    private static AmazonS3Client? CreateS3Client(R2StorageOptions options, ILogger logger) {
        if (OperatingSystem.IsBrowser()) {
            logger.Warning("R2 client is not supported in browser contexts; presigned uploads must be used for writes.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.AccountId) || string.IsNullOrWhiteSpace(options.BucketName)) {
            logger.Warning("R2 account or bucket is missing; content reads and writes may fail.");
            return null;
        }

        bool useAutoRegion = string.IsNullOrWhiteSpace(options.Region)
            || options.Region.Equals("auto", StringComparison.OrdinalIgnoreCase);
        string regionName = useAutoRegion ? "us-east-1" : options.Region;

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
