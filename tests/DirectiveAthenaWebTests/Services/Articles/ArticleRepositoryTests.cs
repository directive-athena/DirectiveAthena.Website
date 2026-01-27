// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Articles;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWebTests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;

namespace DirectiveAthenaWebTests.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleRepositoryTests {
    private static IContentStorage CreateStorage() {
        var storage = Substitute.For<IContentStorage>();
        storage.IndexContentPath.Returns("content/articles/index.json");
        return storage;
    }

    [Test]
    public async Task GetPostsAsync_FiltersOutHiddenPosts() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(1, hidden: false),
            ArticleFaker.Create(2, hidden: true),
            ArticleFaker.Create(3, hidden: false)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(articles), null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

        // Act
        List<Article> result = (await repo.GetAllWithoutHiddenAsync()).ToList();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(p => p.IsHidden)).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_HandlesHttpClientFailure() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ContentReadResult>(new HttpRequestException("boom")));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

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

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(articles), null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

        // Act
        _ = (await repo.GetAllWithoutHiddenAsync()).ToList();
        _ = (await repo.GetAllAsync()).ToList();

        // Assert
        _ = storage.Received(1)
            .ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetPostsAsync_ReturnsEmptyWhenResponseNull() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, "null", null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

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

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(articles), null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

        // Act
        Task[] tasks = Enumerable.Range(0, 5)
            .Select(_ => repo.GetAllAsync().AsTask())
            .ToArray<Task>();
        await Task.WhenAll(tasks);

        // Assert
        _ = storage.Received(1)
            .ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SaveAsync_WritesIndex() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);
        Article[] articles = [ArticleFaker.Create(21)];

        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenWriteFails() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(false));

        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);
        Article[] articles = [ArticleFaker.Create(22)];

        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_SetsTimestampsWhenMissing() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);
        Article article = ArticleFaker.Create(25);
        article.CreatedAt = DateTime.MinValue;
        article.LastModifiedAt = DateTime.MinValue;

        // Act
        bool result = await repo.SaveAsync([article]);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(article.CreatedAt).IsNotEqualTo(DateTime.MinValue);
        await Assert.That(article.LastModifiedAt).IsEqualTo(article.CreatedAt);
    }

    [Test]
    public async Task SaveAsync_PreservesCreatedAt() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);
        Article article = ArticleFaker.Create(26);
        DateTime createdAt = new(2024, 02, 10, 0, 0, 0, DateTimeKind.Utc);
        article.CreatedAt = createdAt;
        article.LastModifiedAt = DateTime.MinValue;

        // Act
        bool result = await repo.SaveAsync([article]);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(article.CreatedAt).IsEqualTo(createdAt);
        await Assert.That(article.LastModifiedAt).IsNotEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        Article article = ArticleFaker.Create(30);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(article.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

        // Act
        bool result = await repo.DeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).DeleteLocalizedFilesAsync(article.MarkdownFileName);
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseWhenLocalizedDeleteFails() {
        // Arrange
        Article article = ArticleFaker.Create(40);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(article.MarkdownFileName).Returns(Task.FromResult(false));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        var logger = Substitute.For<ILogger<ArticleRepository>>();
        var repo = new ArticleRepository(storage, logger);

        // Act
        bool result = await repo.DeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).DeleteLocalizedFilesAsync(article.MarkdownFileName);
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }
}
