// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
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

    private static IFaqContentManager CreateManager(
        ILocalizationProvider localizationProvider,
        IR2Storage<FaqContent>? storage = null
    ) {
        storage ??= Substitute.For<IR2Storage<FaqContent>>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(localizationProvider);

        var factory = Substitute.For<IR2StorageFactory>();
        factory.ForCategory<FaqContent>().Returns(storage);
        services.AddSingleton(factory);

        services.AddFaqContent();

        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IFaqContentManager>();
    }

    [Test]
    public async Task GetLocalizedQuestion_FallsBackToDefaultCulture() {
        // Arrange
        FaqContent rule = ContentFaker.CreateFaq(1, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        IFaqContentManager manager = CreateManager(localizationProvider);

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
        IFaqContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedAnswer(rule);

        // Assert
        await Assert.That(result).IsEqualTo(rule.Answer["en"]);
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var storage = Substitute.For<IR2Storage<FaqContent>>();
        storage.ReadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));
        IFaqContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider(), storage);
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
        IFaqContentManager manager = CreateManager(localizationProvider);

        // Act
        FaqContent rule = manager.Create();
        HashSet<string> expected = localizationProvider.GetSupportedLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(rule.Question.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Answer.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(rule.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(rule.Tags).IsEmpty();
    }
}
