// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Faq.Services;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using NSubstitute;
using DirectiveAthenaWebTests.Helpers;
using Microsoft.Extensions.DependencyInjection;

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
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null
    ) {
        storage ??= Substitute.For<IContentStorage>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(localizationProvider);

        var factory = Substitute.For<IContentStorageFactory>();
        factory.ForCategory(Arg.Any<string>()).Returns(storage);
        factory.ForCategory<FaqContent>().Returns(storage);
        services.AddSingleton(factory);

        services.AddFaqContent();

        ServiceProvider provider = services.BuildServiceProvider();
        return new FaqContentManager(localizationProvider, provider);
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
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var storage = Substitute.For<IContentStorage>();
        storage.ReadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));
        FaqContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider(), storage);
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
}
