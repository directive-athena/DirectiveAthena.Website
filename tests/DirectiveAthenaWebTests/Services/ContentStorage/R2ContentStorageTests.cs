// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using System.Net;
using System.Text;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using NSubstitute;
using DirectiveAthenaWebTests.Helpers;
using System.Reflection;

namespace DirectiveAthenaWebTests.Services.ContentStorage;
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
        IMinioClient? minioClient,
        ILocalizationProvider? localizationProvider = null,
        HttpClient? httpClient = null,
        string categoryFolder = "content/notes",
        string publicBaseUrl = "https://cdn.example.com/",
        bool allowInMemoryFallback = false
    ) {
        localizationProvider ??= CreateLocalizationProvider("en");
        var logger = Substitute.For<ILogger>();
        httpClient ??= new HttpClient(new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        return new R2ContentStorage(localizationProvider, options, categoryFolder, new Uri(publicBaseUrl), httpClient, minioClient, logger, allowInMemoryFallback);
    }

    private static T? GetPrivateFieldValue<T>(object instance, string fieldName) {
        Type? type = instance.GetType();
        while (type is not null) {
            FieldInfo? field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null) {
                return (T?)field.GetValue(instance);
            }

            type = type.BaseType;
        }

        return default;
    }

    private static bool HasObjectName(RemoveObjectArgs request, string suffix) {
        string? key = GetPrivateFieldValue<string>(request, "<ObjectName>k__BackingField");
        return key is not null && key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
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
        var minioClient = Substitute.For<IMinioClient>();
        R2ContentStorage storage = CreateStorage(options, minioClient);

        // Act
        bool result = await storage.WriteFileAsync("content/notes/index.json", "{}");

        // Assert
        await Assert.That(result).IsFalse();
        await minioClient.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectArgs>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WriteFileAsync_ReturnsFalseWhenClientMissing() {
        // Arrange
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), null);

        // Act
        bool result = await storage.WriteFileAsync("content/notes/index.json", "{}");

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task WriteFileAsync_SendsNormalizedJsonRequest() {
        // Arrange
        PutObjectArgs? captured = null;
        var minioClient = Substitute.For<IMinioClient>();
        minioClient.PutObjectAsync(Arg.Do<PutObjectArgs>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse(HttpStatusCode.OK, "etag", new Dictionary<string, string>(), 0, "")));

        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), minioClient);

        // Act
        bool result = await storage.WriteFileAsync("/content/notes/index.json", "{ \"ok\": true }");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(captured is not null).IsTrue();
        await Assert.That(GetPrivateFieldValue<string>(captured!, "<ObjectName>k__BackingField")).IsEqualTo("content/notes/index.json");
        await Assert.That(GetPrivateFieldValue<string>(captured!, "<ContentType>k__BackingField")).IsEqualTo("application/json");
        await Assert.That(GetPrivateFieldValue<long>(captured!, "<ObjectSize>k__BackingField")).IsEqualTo(Encoding.UTF8.GetByteCount("{ \"ok\": true }"));
    }

    [Test]
    public async Task WriteFileAsync_SendsMarkdownContentType() {
        // Arrange
        PutObjectArgs? captured = null;
        var minioClient = Substitute.For<IMinioClient>();
        minioClient.PutObjectAsync(Arg.Do<PutObjectArgs>(request => captured = request), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PutObjectResponse(HttpStatusCode.OK, "etag", new Dictionary<string, string>(), 0, "")));

        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), minioClient);

        // Act
        bool result = await storage.WriteFileAsync("content/notes/en/post.md", "# Title");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(captured is not null).IsTrue();
        await Assert.That(GetPrivateFieldValue<string>(captured!, "<ContentType>k__BackingField")).IsEqualTo("text/markdown");
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_DeletesAllLocalizedFiles() {
        // Arrange
        var minioClient = Substitute.For<IMinioClient>();
        var keys = new List<string>();
        minioClient.RemoveObjectAsync(Arg.Do<RemoveObjectArgs>(request => {
                string? key = GetPrivateFieldValue<string>(request, "<ObjectName>k__BackingField");
                if (!string.IsNullOrWhiteSpace(key)) {
                    keys.Add(key);
                }
            }), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en", "nl");
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), minioClient, localizationProvider);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(keys.Count).IsEqualTo(2);
        await Assert.That(keys.Any(key => key == "content/notes/en/post.md")).IsTrue();
        await Assert.That(keys.Any(key => key == "content/notes/nl/post.md")).IsTrue();
    }

    [Test]
    public async Task DeleteLocalizedFilesAsync_ReturnsFalseOnFailure() {
        // Arrange
        var minioClient = Substitute.For<IMinioClient>();
        minioClient.RemoveObjectAsync(
                Arg.Is<RemoveObjectArgs>(request => HasObjectName(request, "/en/post.md")),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromException(new InvalidOperationException("missing")));
        minioClient.RemoveObjectAsync(
                Arg.Is<RemoveObjectArgs>(request => HasObjectName(request, "/nl/post.md")),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en", "nl");
        R2ContentStorage storage = CreateStorage(WriteEnabledOptions(), minioClient, localizationProvider);

        // Act
        bool result = await storage.DeleteLocalizedFilesAsync("post.md");

        // Assert
        await Assert.That(result).IsFalse();
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
        bool result = await storage.WriteFileAsync("content/notes/en/post.md", "# Title");

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
