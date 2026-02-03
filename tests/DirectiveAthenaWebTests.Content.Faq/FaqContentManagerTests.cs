// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DirectiveAthenaWebTests.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class FaqContentManagerTests {
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
        factory.ForCategory("faq").Returns(storage);
        return factory;
    }

    private static FaqContentManager CreateManager(
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null
    ) {
        storage ??= Substitute.For<IContentStorage>();
        var logger = Substitute.For<ILogger<FaqContentManager>>();
        return new FaqContentManager(localizationProvider, CreateStorageFactory(storage), logger);
    }

    private static FaqContent CreateRule(int seed, bool includeNl = true) {
        var rule = new FaqContent {
            Id = Guid.NewGuid(),
            Question = new Dictionary<string, string> {
                ["en"] = $"Question {seed}"
            },
            Answer = new Dictionary<string, string> {
                ["en"] = $"Answer {seed}"
            },
            Tags = [
                "tag-one"
            ],
            InternalTitle = string.Empty
        };

        if (!includeNl) return rule;

        rule.Question["nl"] = $"Vraag {seed}";
        rule.Answer["nl"] = $"Antwoord {seed}";

        return rule;
    }

    [Test]
    public async Task GetLocalizedQuestion_FallsBackToDefaultCulture() {
        // Arrange
        FaqContent rule = CreateRule(1, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedQuestion(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Question["en"]);
    }

    [Test]
    public async Task GetLocalizedAnswer_FallsBackToDefaultCulture() {
        // Arrange
        FaqContent rule = CreateRule(2, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedAnswer(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Answer["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        FaqContent rule = CreateRule(3);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/worldrules/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        FaqContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        string result = manager.GetLocalizedFilePath(rule);

        // Assert
        await Assert.That(result).IsEqualTo($"content/worldrules/nl/{rule.MarkdownFileName}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var storage = Substitute.For<IContentStorage>();
        storage.ReadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));
        FaqContentManager manager = CreateManager(CreateLocalizationProvider("en"), storage);
        FaqContent rule = CreateRule(4);

        // Act
        string result = await manager.GetMarkdownContentAsync(rule, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewRule_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        FaqContent rule = manager.Create();
        HashSet<string> expected = localizationProvider.GetSupportedLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(rule.Question.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Answer.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(rule.Tags).IsEmpty();
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        var sharedId = Guid.NewGuid();
        FaqContent[] rules = [
            CreateRule(10),
            CreateRule(11)
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        FaqContentManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(rules, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        FaqContent rule = CreateRule(20);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        FaqContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(rule, writeToDisk: false);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(localizationProvider.GetSupportedLocalizations().Count);
        await Assert.That(result.WroteAll).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenRequested() {
        // Arrange
        FaqContent rule = CreateRule(21);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        FaqContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(rule, writeToDisk: true);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(localizationProvider.GetSupportedLocalizations().Count);
        await Assert.That(result.WroteAll).IsTrue();
        foreach (LocalizationInfo localization in localizationProvider.GetSupportedLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, rule.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }
}
