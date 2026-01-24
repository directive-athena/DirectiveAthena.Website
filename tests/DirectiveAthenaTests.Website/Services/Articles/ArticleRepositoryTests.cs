// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthenaTests.Website.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DirectiveAthenaTests.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleRepositoryTests {
    private static IContentStorage CreateStorage() {
        var storage = Substitute.For<IContentStorage>();
        storage.IndexContentPath.Returns("content/articles/index.json");
        return storage;
    }

    private static IContentStorageFactory CreateFactory(IContentStorage storage) {
        var factory = Substitute.For<IContentStorageFactory>();
        factory.ForCategory(ContentCategory.Articles).Returns(storage);
        return factory;
    }

    [Test]
    public async Task GetPostsAsync_FiltersOutHiddenPosts() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(1, hidden: false),
            ArticleFaker.Create(2, hidden: true),
            ArticleFaker.Create(3, hidden: false)
        ];

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(articles), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(CreateStorage()), logger);

        // Act
        List<Article> result = (await repo.GetAllWithoutHiddenAsync()).ToList();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(p => p.Hidden)).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_HandlesHttpClientFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => throw new HttpRequestException("boom"));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(CreateStorage()), logger);

        // Act
        IEnumerable<Article> result = await repo.GetAllWithoutHiddenAsync();

        // Assert
        await Assert.That(result.Any()).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_CachesResults() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(10, hidden: false),
            ArticleFaker.Create(11, hidden: true)
        ];

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(articles), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(CreateStorage()), logger);

        // Act
        _ = (await repo.GetAllWithoutHiddenAsync()).ToList();
        _ = (await repo.GetAllAsync()).ToList();

        // Assert
        await Assert.That(handler.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetPostsAsync_ReturnsEmptyWhenResponseNull() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(CreateStorage()), logger);

        // Act
        IEnumerable<Article> result = await repo.GetAllWithoutHiddenAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GetPostsAsync_CachesAcrossConcurrentCalls() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(12, hidden: false),
            ArticleFaker.Create(13, hidden: true)
        ];

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(articles), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(CreateStorage()), logger);

        // Act
        Task[] tasks = Enumerable.Range(0, 5)
            .Select(_ => repo.GetAllAsync().AsTask())
            .ToArray<Task>();
        await Task.WhenAll(tasks);

        // Assert
        await Assert.That(handler.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task SaveAsync_RespectsLocalhostAndPermissions() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(new HttpClient(), CreateFactory(storage), logger);
        Article[] articles = [ArticleFaker.Create(21)];

        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenNotLocalhost() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(false);

        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(new HttpClient(), CreateFactory(storage), logger);
        Article[] articles = [ArticleFaker.Create(22)];

        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        Article article = ArticleFaker.Create(30);
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.HasAccessAsync().Returns(new ValueTask<bool>(true));
        storage.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        storage.DeleteLocalizedFilesAsync(article.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(new[] { article }), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(http, CreateFactory(storage), logger);

        // Act
        bool result = await repo.DeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).DeleteLocalizedFilesAsync(article.MarkdownFileName);
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseWhenNoAccess() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.HasAccessAsync().Returns(new ValueTask<bool>(false));

        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(new HttpClient(), CreateFactory(storage), logger);
        Article article = ArticleFaker.Create(40);

        // Act
        bool result = await repo.DeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().DeleteLocalizedFilesAsync(null!);
    }
}
