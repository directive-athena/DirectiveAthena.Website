// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using DirectiveAthena.Website.Services.WorldRules;
using DirectiveAthenaTests.Website.Helpers;
using FluentValidation;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRuleManagerTests {
    private static ILocalizationProvider CreateLocalizationProvider(string currentCode) {
        LocalizationInfo[] localizations = [
            new("en", "English", "EN", ""),
            new("nl", "Nederlands", "NL", "")
        ];

        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(localizations.First(l => l.Code == "en"));
        localizationProvider.GetCurrentLocalization().Returns(localizations.First(l => l.Code == currentCode));
        localizationProvider.GetSupportedLocalizations().Returns(localizations);
        return localizationProvider;
    }

    private static IContentStorageFactory CreateStorageFactory(IContentStorage storage) {
        var factory = Substitute.For<IContentStorageFactory>();
        factory.ForCategory(ContentCategory.WorldRules).Returns(storage);
        return factory;
    }

    private static WorldRuleManager CreateManager(
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null,
        HttpClient? http = null,
        IValidator<IEnumerable<WorldRule>>? validator = null
    ) {
        storage ??= Substitute.For<IContentStorage>();
        http ??= new HttpClient();
        validator ??= new WorldRuleCollectionValidator(new WorldRuleValidator(localizationProvider));
        return new WorldRuleManager(localizationProvider, CreateStorageFactory(storage), http, validator);
    }

    private static WorldRule CreateRule(int seed, bool includeNl = true) {
        var rule = new WorldRule {
            Id = Guid.NewGuid(),
            Date = $"2026-01-{seed:D2}",
            Question = new Dictionary<string, string> { ["en"] = $"Question {seed}" },
            Answer = new Dictionary<string, string> { ["en"] = $"Answer {seed}" },
            Tags = ["tag-one"]
        };

        if (includeNl) {
            rule.Question["nl"] = $"Vraag {seed}";
            rule.Answer["nl"] = $"Antwoord {seed}";
        }

        return rule;
    }

    [Test]
    public async Task GetLocalizedQuestion_FallsBackToDefaultCulture() {
        // Arrange
        WorldRule rule = CreateRule(1, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        WorldRuleManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedQuestion(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Question["en"]);
    }

    [Test]
    public async Task GetLocalizedAnswer_FallsBackToDefaultCulture() {
        // Arrange
        WorldRule rule = CreateRule(2, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        WorldRuleManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedAnswer(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Answer["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        WorldRule rule = CreateRule(3);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/worldrules/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        WorldRuleManager manager = CreateManager(localizationProvider, storage);

        // Act
        string result = manager.GetLocalizedFilePath(rule);

        // Assert
        await Assert.That(result).IsEqualTo($"content/worldrules/nl/{rule.MarkdownFileName}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        WorldRuleManager manager = CreateManager(CreateLocalizationProvider("en"), http: http);
        WorldRule rule = CreateRule(4);

        // Act
        string result = await manager.GetRawMarkdownContentAsync(rule, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewRule_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        WorldRuleManager manager = CreateManager(localizationProvider);

        // Act
        WorldRule rule = manager.NewRule();
        HashSet<string> expected = localizationProvider.GetSupportedLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(rule.Question.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Answer.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(rule.Date).IsNotNullOrWhiteSpace();
        await Assert.That(rule.Tags).IsEmpty();
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        Guid sharedId = Guid.NewGuid();
        WorldRule[] rules = [
            CreateRule(10),
            CreateRule(11)
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        WorldRuleManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(rules, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        WorldRule rule = CreateRule(20);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        WorldRuleManager manager = CreateManager(localizationProvider, storage);

        // Act
        Dictionary<string, string> stubs = await manager.GenerateStubsAsync(rule, writeToDisk: false);

        // Assert
        await Assert.That(stubs.Count).IsEqualTo(localizationProvider.GetSupportedLocalizations().Count);
        await storage.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenLocalhostAndPermitted() {
        // Arrange
        WorldRule rule = CreateRule(21);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        storage.IsLocalhost.Returns(true);
        storage.HasAccessAsync().Returns(new ValueTask<bool>(true));
        storage.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        WorldRuleManager manager = CreateManager(localizationProvider, storage);

        // Act
        Dictionary<string, string> stubs = await manager.GenerateStubsAsync(rule, writeToDisk: true);

        // Assert
        await Assert.That(stubs.Count).IsEqualTo(localizationProvider.GetSupportedLocalizations().Count);
        foreach (LocalizationInfo localization in localizationProvider.GetSupportedLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, rule.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }
}
