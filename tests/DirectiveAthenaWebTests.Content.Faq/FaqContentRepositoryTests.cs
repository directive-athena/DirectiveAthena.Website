// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DirectiveAthenaWebTests.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class FaqContentRepositoryTests {
    private static IContentStorage CreateStorage() {
        var storage = Substitute.For<IContentStorage>();
        storage.IndexContentPath.Returns("content/worldrules/index.json");
        return storage;
    }

    private static FaqContent CreateRule(int seed) => new() {
        Id = Guid.NewGuid(),
        Date = $"2026-01-{seed:D2}",
        Question = new Dictionary<string, string> { ["en"] = $"Question {seed}" },
        Answer = new Dictionary<string, string> { ["en"] = $"Answer {seed}" }
    };

    [Test]
    public async Task GetRulesAsync_HandlesHttpClientFailure() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ContentReadResult>(new HttpRequestException("boom")));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        IEnumerable<FaqContent> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result.Any()).IsFalse();
    }

    [Test]
    public async Task GetRulesAsync_CachesResults() {
        // Arrange
        FaqContent[] rules = [
            CreateRule(1),
            CreateRule(2)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(rules), null, null)));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        _ = (await repo.GetAllAsync()).ToList();
        _ = (await repo.GetAllAsync()).ToList();

        // Assert
        _ = storage.Received(1)
            .ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetRulesAsync_ReturnsEmptyWhenResponseNull() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, "null", null, null)));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        IEnumerable<FaqContent> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNullForSoftDeleted() {
        // Arrange
        FaqContent rule = CreateRule(3);
        rule.SoftDeletedAt = DateTime.UtcNow;
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        FaqContent? result = await repo.GetByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task SaveAsync_WritesIndex() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);
        FaqContent[] rules = [CreateRule(10)];

        // Act
        bool result = await repo.SaveAsync(rules);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenWriteFails() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(false));

        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);
        FaqContent[] rules = [CreateRule(11)];

        // Act
        bool result = await repo.SaveAsync(rules);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_SetsTimestampsWhenMissing() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);
        FaqContent rule = CreateRule(12);
        rule.CreatedAt = DateTime.MinValue;
        rule.LastModifiedAt = DateTime.MinValue;

        // Act
        bool result = await repo.SaveAsync([rule]);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(rule.CreatedAt).IsNotEqualTo(DateTime.MinValue);
        await Assert.That(rule.LastModifiedAt).IsEqualTo(rule.CreatedAt);
    }

    [Test]
    public async Task SaveAsync_PreservesCreatedAt() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);
        FaqContent rule = CreateRule(13);
        DateTime createdAt = new(2024, 02, 10, 0, 0, 0, DateTimeKind.Utc);
        rule.CreatedAt = createdAt;
        rule.LastModifiedAt = DateTime.MinValue;

        // Act
        bool result = await repo.SaveAsync([rule]);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(rule.CreatedAt).IsEqualTo(createdAt);
        await Assert.That(rule.LastModifiedAt).IsNotEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task SoftDeleteByIdAsync_MarksDeletedAndWritesIndex() {
        // Arrange
        FaqContent rule = CreateRule(14);
        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        string? capturedJson = null;
        storage.WriteIndexAsync(Arg.Do<string>(json => capturedJson = json)).Returns(new ValueTask<bool>(true));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        bool result = await repo.SoftDeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(capturedJson).IsNotNull();
        FaqContent[]? saved = JsonSerializer.Deserialize<FaqContent[]>(
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
        FaqContent rule = CreateRule(20);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(rule.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        bool result = await repo.DeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).DeleteLocalizedFilesAsync(rule.MarkdownFileName);
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseWhenLocalizedDeleteFails() {
        // Arrange
        FaqContent rule = CreateRule(30);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(rule.MarkdownFileName).Returns(Task.FromResult(false));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        var logger = Substitute.For<ILogger<FaqContentRepository>>();
        var repo = new FaqContentRepository(storage, logger);

        // Act
        bool result = await repo.DeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).DeleteLocalizedFilesAsync(rule.MarkdownFileName);
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }
}
