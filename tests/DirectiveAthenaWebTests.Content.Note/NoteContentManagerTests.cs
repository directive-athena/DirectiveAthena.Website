// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using NSubstitute;
using DirectiveAthenaWebTests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaWebTests.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContentManagerTests {
    // private static IContentStorageFactory CreateStorageFactory(IContentStorage storage) {
    //     var factory = Substitute.For<IContentStorageFactory>();
    //     factory.ForCategory("note").Returns(storage);
    //     factory.ForCategory<NoteContent>().Returns(storage);
    //     return factory;
    // }

    private static INoteContentManager CreateManager(
        ILocalizationProvider localizationProvider,
        IR2Storage<NoteContent>? storage = null
    ) {
        storage ??= Substitute.For<IR2Storage<NoteContent>>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(localizationProvider);

        var factory = Substitute.For<IR2StorageFactory>();
        factory.ForCategory<NoteContent>().Returns(storage);
        services.AddSingleton(factory);

        services.AddNoteContent(out _);

        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<INoteContentManager>();
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetLocalizedTitle_FallsBackToDefaultCulture() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(100, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        INoteContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedTitle(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.LocalizedTitles["en"]);
    }

    [Test]
    public async Task GetLocalizedSummary_FallsBackToDefaultCulture() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(101, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        INoteContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedSummary(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.LocalizedSummaries["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(102);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        var storage = Substitute.For<IR2Storage<NoteContent>>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/notes/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        INoteContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        string result = manager.GetLocalizedMarkdownFilePath(article);

        // Assert
        await Assert.That(result).IsEqualTo($"content/notes/nl/{article.MarkdownFileName}");
    }

    [Test]
    public async Task GetMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var storage = Substitute.For<IR2Storage<NoteContent>>();
        storage.ReadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));
        INoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider(), storage);
        NoteContent article = ContentFaker.CreateNote(103);

        // Act
        string result = await manager.GetMarkdownContentAsync(article, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewWriting_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider();
        INoteContentManager manager = CreateManager(localizationProvider);

        // Act
        NoteContent article = manager.Create();
        HashSet<string> expected = TestLocalization.DefaultLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(article.LocalizedTitles.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.LocalizedSummaries.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(article.MarkdownFileName).IsNotNullOrWhiteSpace();
    }
}
