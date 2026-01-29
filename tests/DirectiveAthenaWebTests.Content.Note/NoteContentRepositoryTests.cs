// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Content.Note.Services;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DirectiveAthenaWebTests.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContentRepositoryTests {
    private static IContentStorage CreateStorage() {
        var storage = Substitute.For<IContentStorage>();
        storage.IndexContentPath.Returns("content/notes/index.json");
        return storage;
    }

    [Test]
    public async Task GetPostsAsync_FiltersOutHiddenPosts() {
        // Arrange
        NoteContent[] notes = [
            NoteFaker.Create(1, hidden: false),
            NoteFaker.Create(2, hidden: true),
            NoteFaker.Create(3, hidden: false)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        List<NoteContent> result = (await repo.GetAllAsync()).ToList();

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
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        IEnumerable<NoteContent> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result.Any()).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_CachesResults() {
        // Arrange
        NoteContent[] notes = [
            NoteFaker.Create(10, hidden: false),
            NoteFaker.Create(11, hidden: true)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        _ = (await repo.GetAllAsync()).ToList();
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
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        IEnumerable<NoteContent> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNullForSoftDeleted() {
        // Arrange
        NoteContent article = NoteFaker.Create(6);
        article.SoftDeletedAt = DateTime.UtcNow;
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent? result = await repo.GetByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetPostsAsync_CachesAcrossConcurrentCalls() {
        // Arrange
        NoteContent[] notes = [
            NoteFaker.Create(12, hidden: false),
            NoteFaker.Create(13, hidden: true)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

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
    public async Task GetAllAsync_DefaultFiltersHiddenAndSoftDeleted() {
        // Arrange
        NoteContent visible = NoteFaker.Create(50, hidden: false);
        NoteContent hidden = NoteFaker.Create(51, hidden: true);
        NoteContent softDeleted = NoteFaker.Create(52, hidden: false);
        softDeleted.SoftDeletedAt = DateTime.UtcNow;

        NoteContent[] notes = [visible, hidden, softDeleted];
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent[] result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result).HasSingleItem();
        await Assert.That(result.Single().Id).IsEqualTo(visible.Id);
    }

    [Test]
    public async Task GetAllAsync_WithHidden_IncludesHiddenButNotSoftDeleted() {
        // Arrange
        NoteContent visible = NoteFaker.Create(53, hidden: false);
        NoteContent hidden = NoteFaker.Create(54, hidden: true);
        NoteContent softDeleted = NoteFaker.Create(55, hidden: false);
        softDeleted.SoftDeletedAt = DateTime.UtcNow;

        NoteContent[] notes = [visible, hidden, softDeleted];
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent[] result = await repo.GetAllAsync(QueryConfig.WithHidden);

        // Assert
        await Assert.That(result.Select(r => r.Id)).IsEquivalentTo(new[] { visible.Id, hidden.Id });
    }

    [Test]
    public async Task GetAllAsync_WithSoftDeleted_IncludesSoftDeletedButNotHidden() {
        // Arrange
        NoteContent visible = NoteFaker.Create(56, hidden: false);
        NoteContent hidden = NoteFaker.Create(57, hidden: true);
        NoteContent softDeleted = NoteFaker.Create(58, hidden: false);
        softDeleted.SoftDeletedAt = DateTime.UtcNow;

        NoteContent[] notes = [visible, hidden, softDeleted];
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent[] result = await repo.GetAllAsync(QueryConfig.WithSoftDeleted);

        // Assert
        await Assert.That(result.Select(r => r.Id)).IsEquivalentTo(new[] { visible.Id, softDeleted.Id });
    }

    [Test]
    public async Task GetAllAsync_WithHiddenAndSoftDeleted_IncludesAll() {
        // Arrange
        NoteContent visible = NoteFaker.Create(59, hidden: false);
        NoteContent hidden = NoteFaker.Create(60, hidden: true);
        NoteContent softDeleted = NoteFaker.Create(61, hidden: false);
        softDeleted.SoftDeletedAt = DateTime.UtcNow;

        NoteContent[] notes = [visible, hidden, softDeleted];
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent[] result = await repo.GetAllAsync(QueryConfig.WithHidden | QueryConfig.WithSoftDeleted);

        // Assert
        await Assert.That(result.Select(r => r.Id)).IsEquivalentTo(new[] { visible.Id, hidden.Id, softDeleted.Id });
    }

    [Test]
    public async Task GetAllAsync_SortsByCreatedAtAndReverses() {
        // Arrange
        NoteContent first = NoteFaker.Create(62, hidden: false);
        first.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        NoteContent second = NoteFaker.Create(63, hidden: false);
        second.CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        NoteContent third = NoteFaker.Create(64, hidden: false);
        third.CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        NoteContent[] notes = [second, third, first];
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(notes), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        NoteContent[] result = await repo.GetAllAsync(QueryConfig.SortByCreatedAt | QueryConfig.Reversed);

        // Assert
        await Assert.That(result.Select(r => r.Id)).IsEquivalentTo(new[] { third.Id, second.Id, first.Id });
    }

    [Test]
    public async Task SaveAsync_WritesIndex() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);
        NoteContent[] notes = [NoteFaker.Create(21)];

        // Act
        bool result = await repo.SaveAsync(notes);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenWriteFails() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(false));

        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);
        NoteContent[] notes = [NoteFaker.Create(22)];

        // Act
        bool result = await repo.SaveAsync(notes);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_SetsTimestampsWhenMissing() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);
        NoteContent article = NoteFaker.Create(25);
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
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);
        NoteContent article = NoteFaker.Create(26);
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
    public async Task SoftDeleteByIdAsync_MarksDeletedAndWritesIndex() {
        // Arrange
        NoteContent article = NoteFaker.Create(27);
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        string? capturedJson = null;
        storage.WriteIndexAsync(Arg.Do<string>(json => capturedJson = json)).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        bool result = await repo.SoftDeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(capturedJson).IsNotNull();
        NoteContent[]? saved = JsonSerializer.Deserialize<NoteContent[]>(
            capturedJson!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        await Assert.That(saved).IsNotNull();
        await Assert.That(saved!.Single().IsSoftDeleted).IsTrue();
        await Assert.That(saved!.Single().SoftDeletedAt).IsNotEqualTo(DateTime.MinValue);
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        NoteContent article = NoteFaker.Create(30);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(article.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

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
        NoteContent article = NoteFaker.Create(40);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(article.MarkdownFileName).Returns(Task.FromResult(false));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { article }), null, null)));
        var logger = Substitute.For<ILogger<NoteContentRepository>>();
        var repo = new NoteContentRepository(storage, logger);

        // Act
        bool result = await repo.DeleteByIdAsync(article.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).DeleteLocalizedFilesAsync(article.MarkdownFileName);
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }
}
