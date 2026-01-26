// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.ContentStorage;
using DirectiveAthena.Website.DevServer;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using MudBlazor.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);
builder.Services.Configure<R2StorageOptions>(builder.Configuration.GetSection("ContentStorage:R2"));
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());
builder.Services.AddWebsiteServices();
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
app.UseAntiforgery();

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

app.MapRazorComponents<AdminApp>()
    .AddInteractiveServerRenderMode();

app.MapFallbackToFile("index.html");
app.Run();
return;

// ---------------------------------------------------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------------------------------------------------
static IMinioClient CreateMinioClient(R2StorageOptions options, ILogger logger) {
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

[UsedImplicitly] internal record ProxyUploadRequest(string Key, string Content, string ContentType);
[UsedImplicitly] internal record ProxyDeleteRequest(string Key);
