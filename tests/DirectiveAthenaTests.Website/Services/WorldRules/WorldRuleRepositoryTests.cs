// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.ContentStorage;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using DirectiveAthena.Website.Services.WorldRules;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRuleRepositoryTests {
    private static IContentStorage CreateStorage() {
        var storage = Substitute.For<IContentStorage>();
        storage.IndexContentPath.Returns("content/worldrules/index.json");
        return storage;
    }

    private static IContentStorageFactory CreateFactory(IContentStorage storage) {
        var factory = Substitute.For<IContentStorageFactory>();
        factory.ForCategory(ContentCategory.WorldRules).Returns(storage);
        return factory;
    }

    private static WorldRule CreateRule(int seed) => new() {
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
        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);

        // Act
        IEnumerable<WorldRule> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result.Any()).IsFalse();
    }

    [Test]
    public async Task GetRulesAsync_CachesResults() {
        // Arrange
        WorldRule[] rules = [
            CreateRule(1),
            CreateRule(2)
        ];

        IContentStorage storage = CreateStorage();
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(rules), null, null)));
        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);

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
        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);

        // Act
        IEnumerable<WorldRule> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task SaveAsync_WritesIndex() {
        // Arrange
        IContentStorage storage = CreateStorage();
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);
        WorldRule[] rules = [CreateRule(10)];

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

        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);
        WorldRule[] rules = [CreateRule(11)];

        // Act
        bool result = await repo.SaveAsync(rules);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        WorldRule rule = CreateRule(20);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(rule.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);

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
        WorldRule rule = CreateRule(30);
        IContentStorage storage = CreateStorage();
        storage.DeleteLocalizedFilesAsync(rule.MarkdownFileName).Returns(Task.FromResult(false));
        storage.ReadIndexAsync(Arg.Any<EntityTagHeaderValue?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ContentReadResult(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { rule }), null, null)));
        var logger = Substitute.For<ILogger<WorldRuleRepository>>();
        var repo = new WorldRuleRepository(CreateFactory(storage), logger);

        // Act
        bool result = await repo.DeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.Received(1).DeleteLocalizedFilesAsync(rule.MarkdownFileName);
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }
}
