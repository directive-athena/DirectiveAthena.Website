// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.ContentStorage;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

ILogger startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
R2StorageOptions r2Options = app.Services.GetRequiredService<IOptions<R2StorageOptions>>().Value;
startupLogger.LogInformation(
    "R2 config: AccountId={AccountId}, BucketName={Bucket}, Region={Region}, EnableWrites={EnableWrites}, HasAccessKey={HasAccessKey}, HasSecret={HasSecret}",
    r2Options.AccountId ?? "(missing)",
    r2Options.BucketName ?? "(missing)",
    r2Options.Region.IsNullOrWhiteSpace() ? "(missing)" : r2Options.Region,
    r2Options.EnableWrites,
    !r2Options.AccessKeyId.IsNullOrWhiteSpace(),
    !r2Options.SecretAccessKey.IsNullOrWhiteSpace()
);
if (!r2Options.AccessKeyId.IsNullOrWhiteSpace()) {
    string accessKey = r2Options.AccessKeyId!;
    string tail = accessKey.Length <= 4 ? accessKey : accessKey[^4..];
    startupLogger.LogInformation("R2 AccessKeyId length={Length}, tail=****{Tail}", accessKey.Length, tail);
}

app.MapGet("/config", (IOptions<R2StorageOptions> optionsAccessor) => {
    R2StorageOptions options = optionsAccessor.Value;
    return Results.Ok(new {
        AccountId = options.AccountId ?? "(missing)",
        BucketName = options.BucketName ?? "(missing)",
        Region = options.Region.IsNullOrWhiteSpace() ? "(missing)" : options.Region,
        EnableWrites = options.EnableWrites,
        HasAccessKey = !options.AccessKeyId.IsNullOrWhiteSpace(),
        HasSecret = !options.SecretAccessKey.IsNullOrWhiteSpace()
    });
});

app.MapGet("/probe", async (
    HttpRequest request,
    IOptions<R2StorageOptions> optionsAccessor,
    ILoggerFactory loggerFactory
) => {
    ILogger logger = loggerFactory.CreateLogger("Probe");
    R2StorageOptions options = optionsAccessor.Value;

    try {
        if (!options.IsWriteConfigured) {
            return Results.Problem("R2 credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
        }

        IMinioClient minioClient = CreateMinioClient(options, logger);
        var existsArgs = new BucketExistsArgs()
            .WithBucket(options.BucketName!);
        bool exists = await minioClient.BucketExistsAsync(existsArgs, request.HttpContext.RequestAborted);
        return Results.Ok(new {
            BucketExists = exists
        });
    }
    catch (Exception ex) {
        logger.LogWarning(ex, "R2 probe failed.");
        return Results.Problem("R2 probe failed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/presign", async (
    HttpRequest request,
    IOptions<R2StorageOptions> optionsAccessor,
    ILoggerFactory loggerFactory
) => {
    ILogger logger = loggerFactory.CreateLogger("Presign");
    R2StorageOptions options = optionsAccessor.Value;

    try {
        if (!options.IsWriteConfigured) {
            return Results.Problem("R2 credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
        }

        PresignRequest? payload = await request.ReadFromJsonAsync<PresignRequest>(request.HttpContext.RequestAborted);
        if (payload is null || payload.Key.IsNullOrWhiteSpace() || payload.Method.IsNullOrWhiteSpace()) {
            return Results.BadRequest("Key and method are required.");
        }

        if (payload.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase)) {
            return Results.BadRequest("DELETE presign is not supported; use /delete instead.");
        }

        IMinioClient minioClient = CreateMinioClient(options, logger);
        var presignArgs = new PresignedPutObjectArgs()
            .WithBucket(options.BucketName!)
            .WithObject(payload.Key)
            .WithExpiry(10 * 60);

        string url = await minioClient.PresignedPutObjectAsync(presignArgs);
        logger.LogInformation("Generated presigned URL for {Key}: {Url}", payload.Key, url);
        return Results.Ok(new PresignResponse(url, new Dictionary<string, string>()));
    }
    catch (Exception ex) {
        logger.LogWarning(ex, "Failed to create presigned URL.");
        return Results.Problem("Failed to create presigned URL.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapPost("/upload", async (
    HttpRequest request,
    IOptions<R2StorageOptions> optionsAccessor,
    ILoggerFactory loggerFactory
) => {
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
        var putArgs = new PutObjectArgs()
            .WithBucket(options.BucketName!)
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
});

app.MapPost("/delete", async (
    HttpRequest request,
    IOptions<R2StorageOptions> optionsAccessor,
    ILoggerFactory loggerFactory
) => {
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
        var deleteArgs = new RemoveObjectArgs()
            .WithBucket(options.BucketName!)
            .WithObject(payload.Key);

        await minioClient.RemoveObjectAsync(deleteArgs, request.HttpContext.RequestAborted);
        return Results.Ok();
    }
    catch (Exception ex) {
        logger.LogWarning(ex, "Proxy delete failed.");
        return Results.Problem("Delete failed.", statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapFallbackToFile("index.html");
app.Run();
return;

// ---------------------------------------------------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------------------------------------------------
static IMinioClient CreateMinioClient(R2StorageOptions options, ILogger logger) {
    if (options.AccountId.IsNullOrWhiteSpace() || options.BucketName.IsNullOrWhiteSpace()) {
        throw new InvalidOperationException("R2 account and bucket must be configured for presign.");
    }

    if (options.AccessKeyId.IsNullOrWhiteSpace() || options.SecretAccessKey.IsNullOrWhiteSpace()) {
        throw new InvalidOperationException("R2 access keys must be configured for presign.");
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

[UsedImplicitly] internal sealed record PresignRequest(string Key, string Method, string? ContentType);
[UsedImplicitly] internal sealed record PresignResponse(string Url, Dictionary<string, string> Headers);
[UsedImplicitly] internal sealed record ProxyUploadRequest(string Key, string Content, string ContentType);
[UsedImplicitly] internal sealed record ProxyDeleteRequest(string Key);
