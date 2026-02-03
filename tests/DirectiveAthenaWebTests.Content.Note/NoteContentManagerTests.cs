// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Content.Note.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DirectiveAthenaWebTests.Helpers;

namespace DirectiveAthenaWebTests.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContentManagerTests {
    private static IContentStorageFactory CreateStorageFactory(IContentStorage storage) {
        var factory = Substitute.For<IContentStorageFactory>();
        factory.ForCategory("note").Returns(storage);
        factory.ForCategory<NoteContent>().Returns(storage);
        return factory;
    }

    private static NoteContentManager CreateManager(
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null
    ) {
        storage ??= Substitute.For<IContentStorage>();
        var logger = Substitute.For<ILogger<NoteContentManager>>();
        var manager = new NoteContentManager(localizationProvider, CreateStorageFactory(storage), logger);
        manager.SingleValidator = new NoteContentValidator(localizationProvider);
        manager.MultipleValidator = new NoteContentCollectionValidator(manager.SingleValidator);
        return manager;
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetLocalizedTitle_FallsBackToDefaultCulture() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(100, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        NoteContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedTitle(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Title["en"]);
    }

    [Test]
    public async Task GetLocalizedSummary_FallsBackToDefaultCulture() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(101, includeNl: false);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        NoteContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedSummary(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Summary["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(102);
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/notes/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        NoteContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        string result = manager.GetLocalizedFilePath(article);

        // Assert
        await Assert.That(result).IsEqualTo($"content/notes/nl/{article.MarkdownFileName}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var storage = Substitute.For<IContentStorage>();
        storage.ReadFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>((string?)null));
        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"), storage);
        NoteContent article = ContentFaker.CreateNote(103);

        // Act
        string result = await manager.GetRawMarkdownContentAsync(article, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewWriting_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = TestLocalization.CreateLocalizationProvider("en");
        NoteContentManager manager = CreateManager(localizationProvider);

        // Act
        NoteContent article = manager.Create();
        HashSet<string> expected = TestLocalization.DefaultLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(article.Title.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Summary.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(article.MarkdownFileName).IsNotNullOrWhiteSpace();
    }

    [Test]
    public async Task Validate_RejectsMissingId() {
        // Arrange
        NoteContent[] notes = [
            new() {
                Id = Guid.Empty,
                InternalTitle = string.Empty
            }
        ];

        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(notes, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing Id!");
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        NoteContent[] notes = [
            ContentFaker.CreateNote(140),
            ContentFaker.CreateNote(141)
        ];
        notes[1].Id = notes[0].Id;

        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(notes, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedTitles() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(200, includeNl: false);
        NoteContent[] notes = [article];

        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(notes, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing titles for one or more cultures!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedSummaries() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(201);
        article.Summary.Remove("nl");
        NoteContent[] notes = [article];

        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(notes, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing summaries for one or more cultures!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(120);
        var storage = Substitute.For<IContentStorage>();
        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"), storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: false);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(TestLocalization.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!, default);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenRequested() {
        // Arrange
        NoteContent article = ContentFaker.CreateNote(121);
        var storage = Substitute.For<IContentStorage>();
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        NoteContentManager manager = CreateManager(TestLocalization.CreateLocalizationProvider("en"), storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: true);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(TestLocalization.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsTrue();
        foreach (LocalizationInfo localization in TestLocalization.DefaultLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, article.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }

}
