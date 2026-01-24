// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Text;
using System.Text.Json;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.WorldRules;
using DirectiveAthenaTests.Website.Helpers;
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
        var handler = new TestHttpMessageHandler(_ => throw new HttpRequestException("boom"));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new WorldRuleRepository(http, CreateFactory(CreateStorage()));

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

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(rules), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new WorldRuleRepository(http, CreateFactory(CreateStorage()));

        // Act
        _ = (await repo.GetAllAsync()).ToList();
        _ = (await repo.GetAllAsync()).ToList();

        // Assert
        await Assert.That(handler.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetRulesAsync_ReturnsEmptyWhenResponseNull() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new WorldRuleRepository(http, CreateFactory(CreateStorage()));

        // Act
        IEnumerable<WorldRule> result = await repo.GetAllAsync();

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task SaveAsync_RespectsLocalhostAndPermissions() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var repo = new WorldRuleRepository(new HttpClient(), CreateFactory(storage));
        WorldRule[] rules = [CreateRule(10)];

        // Act
        bool result = await repo.SaveAsync(rules);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenNotLocalhost() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(false);

        var repo = new WorldRuleRepository(new HttpClient(), CreateFactory(storage));
        WorldRule[] rules = [CreateRule(11)];

        // Act
        bool result = await repo.SaveAsync(rules);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteIndexAsync(null!);
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        WorldRule rule = CreateRule(20);
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.HasAccessAsync().Returns(new ValueTask<bool>(true));
        storage.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        storage.DeleteLocalizedFilesAsync(rule.MarkdownFileName).Returns(Task.FromResult(true));
        storage.WriteIndexAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(new[] { rule }), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new WorldRuleRepository(http, CreateFactory(storage));

        // Act
        bool result = await repo.DeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsTrue();
        await storage.Received(1).DeleteLocalizedFilesAsync(rule.MarkdownFileName);
        await storage.Received(1).WriteIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseWhenNoAccess() {
        // Arrange
        var storage = CreateStorage();
        storage.IsLocalhost.Returns(true);
        storage.HasAccessAsync().Returns(new ValueTask<bool>(false));

        var repo = new WorldRuleRepository(new HttpClient(), CreateFactory(storage));
        WorldRule rule = CreateRule(30);

        // Act
        bool result = await repo.DeleteByIdAsync(rule.Id);

        // Assert
        await Assert.That(result).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().DeleteLocalizedFilesAsync(null!);
    }
}
