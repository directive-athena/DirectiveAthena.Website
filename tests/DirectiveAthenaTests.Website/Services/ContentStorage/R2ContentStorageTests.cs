// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using DirectiveAthena.Website.Services.ContentStorage;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DirectiveAthenaTests.Website.Helpers;

namespace DirectiveAthenaTests.Website.Services.ContentStorage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2ContentStorageTests {
    private static R2StorageOptions WriteEnabledOptions() => new() {
        EnableWrites = true,
        AccountId = "account",
        AccessKeyId = "access",
        SecretAccessKey = "secret",
        BucketName = "bucket",
        PublicBaseUrl = "https://cdn.example.com/"
    };

    private static ILocalizationProvider CreateLocalizationProvider(params string[] codes) {
        var provider = Substitute.For<ILocalizationProvider>();
        LocalizationInfo[] localizations = codes
            .Select(code => new LocalizationInfo(code, code.ToUpperInvariant(), code.ToUpperInvariant(), $"flags/{code}.png"))
            .ToArray();
        provider.GetSupportedLocalizations().Returns(localizations);
        return provider;
    }

    private static R2ContentStorage CreateStorage(
        R2StorageOptions options,
        IAmazonS3? s3Client,
        ILocalizationProvider? localizationProvider = null,
        HttpClient? httpClient = null,
        string categoryFolder = "content/articles",
        string publicBaseUrl = "https://cdn.example.com/"
    ) {
        localizationProvider ??= CreateLocalizationProvider("en");
        var logger = Substitute.For<ILogger>();
        httpClient ??= new HttpClient(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        return new R2ContentStorage(localizationProvider, options, categoryFolder, new Uri(publicBaseUrl), httpClient, s3Client, logger);
    }

    [Test]
    public async Task WriteFileAsync_ReturnsFalseWhenWritesDisabled() {
        // Arrange
        var options = new R2StorageOptions {
            EnableWrites = false,
            AccountId = "account",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket"
        };
        var s3Client = Substitute.For<IAmazonS3>();
        R2ContentStorage storage = CreateStorage(options, s3Client);

        // Act
        bool result = await storage.WriteFileAsync("content/articles/index.json", "{}");

        // Assert
        await Assert.That(result).IsFalse();
        await s3Client.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WriteFileAsync_ReturnsFalseWhenClientMissing() {
        // Arrange
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), null);

        // Act
        bool result = await storage.WriteFileAsync("content/articles/index.json", "{}");

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task WriteFileAsync_SendsNormalizedJsonRequest() {
        // Arrange
        PutObjectRequest? captured = null;
        var s3Client = Substitute.For<IAmazonS3>();
        s3Client.PutObjectAsync(Arg.Do<PutObjectRequest>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), s3Client);

        // Act
        bool result = await storage.WriteFileAsync("/content/articles/index.json", "{ \"ok\": true }");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(captured is not null).IsTrue();
        await Assert.That(captured!.Key).IsEqualTo("content/articles/index.json");
        await Assert.That(captured.ContentType).IsEqualTo("application/json");
        await Assert.That(captured.ContentBody).IsEqualTo("{ \"ok\": true }");
    }

    [Test]
    public async Task WriteFileAsync_SendsMarkdownContentType() {
        // Arrange
        PutObjectRequest? captured = null;
        var s3Client = Substitute.For<IAmazonS3>();
        s3Client.PutObjectAsync(Arg.Do<PutObjectRequest>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK }));

        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), s3Client);

        // Act
        bool result = await storage.WriteFileAsync("content/articles/en/post.md", "# Title");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(captured is not null).IsTrue();
        await Assert.That(captured!.ContentType).IsEqualTo("text/markdown");
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_DeletesAllLocalizedFiles() {
        // Arrange
        var s3Client = Substitute.For<IAmazonS3>();
        var keys = new List<string>();
        s3Client.DeleteObjectAsync(Arg.Do<DeleteObjectRequest>(request => keys.Add(request.Key)), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent }));

        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en", "nl");
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), s3Client, localizationProvider);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(keys.Count).IsEqualTo(2);
        await Assert.That(keys.Any(key => key == "content/articles/en/post.md")).IsTrue();
        await Assert.That(keys.Any(key => key == "content/articles/nl/post.md")).IsTrue();
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_TreatsNotFoundAsSuccess() {
        // Arrange
        var s3Client = Substitute.For<IAmazonS3>();
        s3Client.DeleteObjectAsync(
                Arg.Is<DeleteObjectRequest>(request => request.Key.EndsWith("/en/post.md", StringComparison.OrdinalIgnoreCase)),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromException<DeleteObjectResponse>(new AmazonS3Exception("missing") { StatusCode = HttpStatusCode.NotFound }));
        s3Client.DeleteObjectAsync(
                Arg.Is<DeleteObjectRequest>(request => request.Key.EndsWith("/nl/post.md", StringComparison.OrdinalIgnoreCase)),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(new DeleteObjectResponse { HttpStatusCode = HttpStatusCode.NoContent }));

        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en", "nl");
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), s3Client, localizationProvider);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task WriteFileAsync_UsesPresignEndpointWhenClientMissing() {
        // Arrange
        var presignPayload = new {
            Url = "https://uploads.example.com/content/articles/en/post.md",
            Headers = new Dictionary<string, string> { ["x-test"] = "ok" }
        };

        var handler = new TestHttpMessageHandler(request => {
            if (request.RequestUri is not null && request.RequestUri.AbsoluteUri.EndsWith("/sign", StringComparison.OrdinalIgnoreCase)) {
                string json = JsonSerializer.Serialize(presignPayload);
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            }

            return request.Method == HttpMethod.Put
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var options = new R2StorageOptions {
            EnableWrites = true,
            AccountId = "account",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket",
            PresignEndpoint = "https://api.example.com/sign"
        };

        R2ContentStorage storage = CreateStorage(options, null, httpClient: httpClient);

        // Act
        bool result = await storage.WriteFileAsync("content/articles/en/post.md", "# Title");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(handler.CallCount).IsEqualTo(2);
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_UsesPresignEndpointWhenClientMissing() {
        // Arrange
        var handler = new TestHttpMessageHandler(request => {
            if (request.RequestUri is not null && request.RequestUri.AbsoluteUri.EndsWith("/sign", StringComparison.OrdinalIgnoreCase)) {
                string json = JsonSerializer.Serialize(new { Url = "https://uploads.example.com/delete" });
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            }

            return request.Method == HttpMethod.Delete
                ? new HttpResponseMessage(HttpStatusCode.NoContent)
                : new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var options = new R2StorageOptions {
            EnableWrites = true,
            AccountId = "account",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket",
            PresignEndpoint = "https://api.example.com/sign"
        };

        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en", "nl");
        R2ContentStorage storage = CreateStorage(options, null, localizationProvider, httpClient);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(handler.CallCount).IsEqualTo(4);
    }

    [Test]
    public async Task WriteFileAsync_UsesProxyEndpointWhenConfigured() {
        // Arrange
        var handler = new TestHttpMessageHandler(request => {
            return request.RequestUri?.AbsoluteUri.EndsWith("/upload", StringComparison.OrdinalIgnoreCase) == true
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var options = new R2StorageOptions {
            EnableWrites = true,
            AccountId = "account",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket",
            ProxyEndpoint = "https://api.example.com"
        };

        var storage = CreateStorage(options, null, httpClient: httpClient);

        // Act
        bool result = await storage.WriteFileAsync("content/articles/en/post.md", "# Title");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(handler.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_UsesProxyEndpointWhenConfigured() {
        // Arrange
        var handler = new TestHttpMessageHandler(request => {
            return request.RequestUri?.AbsoluteUri.EndsWith("/delete", StringComparison.OrdinalIgnoreCase) == true
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com/") };
        var options = new R2StorageOptions {
            EnableWrites = true,
            AccountId = "account",
            AccessKeyId = "access",
            SecretAccessKey = "secret",
            BucketName = "bucket",
            ProxyEndpoint = "https://api.example.com"
        };

        var localizationProvider = CreateLocalizationProvider("en", "nl");
        var storage = CreateStorage(options, null, localizationProvider, httpClient);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(handler.CallCount).IsEqualTo(2);
    }
}
