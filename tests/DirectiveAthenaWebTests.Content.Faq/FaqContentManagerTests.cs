// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using NSubstitute;
using DirectiveAthenaWebTests.Helpers;

namespace DirectiveAthenaWebTests.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class FaqContentManagerTests {
    // private static IContentStorageFactory CreateStorageFactory(IContentStorage storage) {
    //     var factory = Substitute.For<IContentStorageFactory>();
    //     factory.ForCategory("faq").Returns(storage);
    //     factory.ForCategory<FaqContent>().Returns(storage);
    //     return factory;
    // }

    private static FaqContentManager CreateManager(
        ILocalizationProvider localizationProvider
    ) {
        var manager = new FaqContentManager(localizationProvider, Substitute.For<IServiceProvider>());
        return manager;
    }

    [Test]
    public async Task GetLocalizedQuestion_FallsBackToDefaultCulture() {
        // Arrange
        FaqContent rule = ContentFaker.CreateFaq(1, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedQuestion(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Question["en"]);
    }

    [Test]
    public async Task GetLocalizedAnswer_FallsBackToDefaultCulture() {
        // Arrange
        FaqContent rule = ContentFaker.CreateFaq(2, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedAnswer(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Answer["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        FaqContent rule = ContentFaker.CreateFaq(3);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/worldrules/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        FaqContentManager manager = CreateManager(localizationProvider);

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
        FaqContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider());
        FaqContent rule = ContentFaker.CreateFaq(4);

        // Act
        string result = await manager.GetMarkdownContentAsync(rule, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewRule_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider();
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
            ContentFaker.CreateFaq(10),
            ContentFaker.CreateFaq(11)
        ];
        rules[0].Id = sharedId;
        rules[1].Id = sharedId;

        FaqContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider());

        // Act
        bool result = manager.Validate(rules, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        FaqContent rule = ContentFaker.CreateFaq(20);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider();
        FaqContentManager manager = CreateManager(localizationProvider);

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
        FaqContent rule = ContentFaker.CreateFaq(21);
        var storage = Substitute.For<IContentStorage>();
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider();
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        FaqContentManager manager = CreateManager(localizationProvider);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(rule, writeToDisk: true);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(localizationProvider.GetSupportedLocalizations().Count);
        await Assert.That(result.WroteAll).IsTrue();
        foreach (LocalizationInfo localization in localizationProvider.GetSupportedLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, rule.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }
}
