// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.ContentStorage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace DirectiveAthena.WebsiteDevServer;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ApiEndpoints {
    private static IMinioClient CreateMinioClient(R2StorageOptions options, ILogger logger) {
        if (options.AccountId.IsNullOrWhiteSpace() || options.BucketName.IsNullOrWhiteSpace()) {
            throw new InvalidOperationException("R2 account and bucket must be configured for writes.");
        }

        if (options.AccessKeyId.IsNullOrWhiteSpace() || options.SecretAccessKey.IsNullOrWhiteSpace()) {
            throw new InvalidOperationException("R2 access keys must be configured for writes.");
        }

        string endpointHost = $"{options.AccountId}.r2.cloudflarestorage.com";

        IMinioClient client = new MinioClient()
            .WithEndpoint(endpointHost)
            .WithCredentials(options.AccessKeyId, options.SecretAccessKey)
            .WithSSL()
            .Build();

        logger.LogDebug("Created R2 MinIO client with endpoint {Endpoint}.", endpointHost);
        return client;
    }

    public static async Task<IResult> HandleUpload(HttpRequest request, IOptions<R2StorageOptions> optionsAccessor, ILoggerFactory loggerFactory) {
        ILogger logger = loggerFactory.CreateLogger("ProxyUpload");
        R2StorageOptions options = optionsAccessor.Value;

        try {
            if (!options.IsWriteConfigured) {
                return Results.Problem("R2 credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
            }

            ProxyUploadRequest? payload = await request.ReadFromJsonAsync<ProxyUploadRequest>(request.HttpContext.RequestAborted);
            if (payload is null || payload.Key.IsNullOrWhiteSpace()) {
                return Results.BadRequest("Key is required.");
            }

            IMinioClient minioClient = CreateMinioClient(options, logger);
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(payload.Content);
            using var contentStream = new MemoryStream(contentBytes);
            var putArgs = new PutObjectArgs().WithBucket(options.BucketName!)
                .WithObject(payload.Key)
                .WithStreamData(contentStream)
                .WithObjectSize(contentBytes.Length)
                .WithContentType(payload.ContentType.IsNullOrWhiteSpace() ? "text/plain" : payload.ContentType);

            await minioClient.PutObjectAsync(putArgs, request.HttpContext.RequestAborted);
            return Results.Ok();
        }
        catch (Exception ex) {
            logger.LogWarning(ex, "Proxy upload failed.");
            return Results.Problem("Upload failed.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    public static async Task<IResult> HandleDelete(HttpRequest request, IOptions<R2StorageOptions> optionsAccessor, ILoggerFactory loggerFactory) {
        ILogger logger = loggerFactory.CreateLogger("ProxyDelete");
        R2StorageOptions options = optionsAccessor.Value;

        try {
            if (!options.IsWriteConfigured) {
                return Results.Problem("R2 credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
            }

            ProxyDeleteRequest? payload = await request.ReadFromJsonAsync<ProxyDeleteRequest>(request.HttpContext.RequestAborted);
            if (payload is null || payload.Key.IsNullOrWhiteSpace()) {
                return Results.BadRequest("Key is required.");
            }

            IMinioClient minioClient = CreateMinioClient(options, logger);
            var deleteArgs = new RemoveObjectArgs().WithBucket(options.BucketName!)
                .WithObject(payload.Key);

            await minioClient.RemoveObjectAsync(deleteArgs, request.HttpContext.RequestAborted);
            return Results.Ok();
        }
        catch (Exception ex) {
            logger.LogWarning(ex, "Proxy delete failed.");
            return Results.Problem("Delete failed.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

}
