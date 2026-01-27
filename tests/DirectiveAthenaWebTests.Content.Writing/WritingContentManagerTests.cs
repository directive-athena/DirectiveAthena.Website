// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Writing;
using DirectiveAthenaWeb.Content.Writing.Services;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWebTests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace DirectiveAthenaWebTests.Content.Writing;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WritingContentManagerTests {

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
        factory.ForCategory(ContentCategory.Writing).Returns(storage);
        return factory;
    }

    private static WritingContentManager CreateManager(
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null,
        HttpClient? http = null
    ) {
        storage ??= Substitute.For<IContentStorage>();
        http ??= new HttpClient();
        var validator = new WritingContentCollectionValidator(new WritingContentValidator(localizationProvider));
        var logger = Substitute.For<ILogger<WritingContentManager>>();
        return new WritingContentManager(localizationProvider, CreateStorageFactory(storage), http, validator, logger);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetLocalizedTitle_FallsBackToDefaultCulture() {
        // Arrange
        WritingContent article = WritingFaker.Create(100, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        WritingContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedTitle(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Title["en"]);
    }

    [Test]
    public async Task GetLocalizedSummary_FallsBackToDefaultCulture() {
        // Arrange
        WritingContent article = WritingFaker.Create(101, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        WritingContentManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedSummary(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Summary["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        WritingContent article = WritingFaker.Create(102);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/writings/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        WritingContentManager manager = CreateManager(localizationProvider, storage);

        // Act
        string result = manager.GetLocalizedFilePath(article);

        // Assert
        await Assert.That(result).IsEqualTo($"content/writings/nl/{article.MarkdownFileName}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"), http: http);
        WritingContent article = WritingFaker.Create(103);

        // Act
        string result = await manager.GetRawMarkdownContentAsync(article, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewWriting_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        WritingContentManager manager = CreateManager(localizationProvider);

        // Act
        WritingContent article = manager.NewWriting();
        HashSet<string> expected = WritingFaker.DefaultLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(article.Title.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Summary.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(article.MarkdownFileName).IsNotNullOrWhiteSpace();
    }

    [Test]
    public async Task Validate_RejectsMissingId() {
        // Arrange
        WritingContent[] writings = [
            new() { Id = Guid.Empty }
        ];

        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(writings, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing Id!");
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        WritingContent[] writings = [
            WritingFaker.Create(140),
            WritingFaker.Create(141)
        ];
        writings[1].Id = writings[0].Id;

        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(writings, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedTitles() {
        // Arrange
        WritingContent article = WritingFaker.Create(200, includeNl: false);
        WritingContent[] writings = [article];

        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(writings, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing titles for one or more cultures!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedSummaries() {
        // Arrange
        WritingContent article = WritingFaker.Create(201);
        article.Summary.Remove("nl");
        WritingContent[] writings = [article];

        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(writings, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing summaries for one or more cultures!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        WritingContent article = WritingFaker.Create(120);
        var storage = Substitute.For<IContentStorage>();
        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"), storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: false);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(WritingFaker.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenRequested() {
        // Arrange
        WritingContent article = WritingFaker.Create(121);
        var storage = Substitute.For<IContentStorage>();
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        WritingContentManager manager = CreateManager(CreateLocalizationProvider("en"), storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: true);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(WritingFaker.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsTrue();
        foreach (LocalizationInfo localization in WritingFaker.DefaultLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, article.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }

}
