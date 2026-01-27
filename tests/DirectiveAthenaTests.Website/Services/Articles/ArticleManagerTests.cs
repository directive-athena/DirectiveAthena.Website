// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthena.Website.Services.ContentStorage;
using DirectiveAthena.Website.Services.Localization;
using DirectiveAthenaTests.Website.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace DirectiveAthenaTests.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleManagerTests {

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
        factory.ForCategory(ContentCategory.Articles).Returns(storage);
        return factory;
    }

    private static ArticleManager CreateManager(
        ILocalizationProvider localizationProvider,
        IContentStorage? storage = null,
        HttpClient? http = null
    ) {
        storage ??= Substitute.For<IContentStorage>();
        http ??= new HttpClient();
        var validator = new ArticleCollectionValidator(new ArticleValidator(localizationProvider));
        var logger = Substitute.For<ILogger<ArticleManager>>();
        return new ArticleManager(localizationProvider, CreateStorageFactory(storage), http, validator, logger);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetLocalizedTitle_FallsBackToDefaultCulture() {
        // Arrange
        Article article = ArticleFaker.Create(100, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        ArticleManager manager = CreateManager(localizationProvider);

        // Act
        string result = manager.GetLocalizedTitle(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Title["en"]);
    }

    [Test]
    public async Task GetLocalizedSummary_FallsBackToDefaultCulture() {
        // Arrange
        Article article = ArticleFaker.Create(101, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        ArticleManager manager = CreateManager(localizationProvider);
        
        // Act
        string result = manager.GetLocalizedSummary(article);

        // Assert
        await Assert.That(result).IsEqualTo(article.Summary["en"]);
    }

    [Test]
    public async Task GetLocalizedFilePath_UsesCurrentLocalization() {
        // Arrange
        Article article = ArticleFaker.Create(102);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        var storage = Substitute.For<IContentStorage>();
        storage.GetMarkdownContentPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"content/articles/{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");
        ArticleManager manager = CreateManager(localizationProvider, storage);
        
        // Act
        string result = manager.GetLocalizedFilePath(article);

        // Assert
        await Assert.That(result).IsEqualTo($"content/articles/nl/{article.MarkdownFileName}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), http: http);
        Article article = ArticleFaker.Create(103);

        // Act
        string result = await manager.GetRawMarkdownContentAsync(article, "en");

        // Assert
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task NewArticle_PopulatesLocalizedFields() {
        // Arrange
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("en");
        ArticleManager manager = CreateManager(localizationProvider);
        
        // Act
        Article article = manager.NewArticle();
        HashSet<string> expected = ArticleFaker.DefaultLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(article.Title.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Summary.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(article.MarkdownFileName).IsNotNullOrWhiteSpace();
    }

    [Test]
    public async Task Validate_RejectsMissingId() {
        // Arrange
        Article[] articles = [
            new() { Id = Guid.Empty }
        ];

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));
        
        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing Id!");
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(140),
            ArticleFaker.Create(141)
        ];
        articles[1].Id = articles[0].Id;

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedTitles() {
        // Arrange
        Article article = ArticleFaker.Create(200, includeNl: false);
        Article[] articles = [article];

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing titles for one or more cultures!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedSummaries() {
        // Arrange
        Article article = ArticleFaker.Create(201);
        article.Summary.Remove("nl");
        Article[] articles = [article];

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing summaries for one or more cultures!");
    }

    [Test]
    public async Task GenerateStubsAsync_ReturnsStubsForAllCultures() {
        // Arrange
        Article article = ArticleFaker.Create(120);
        var storage = Substitute.For<IContentStorage>();
        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), storage);
        
        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: false);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(ArticleFaker.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsFalse();
        await storage.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenRequested() {
        // Arrange
        Article article = ArticleFaker.Create(121);
        var storage = Substitute.For<IContentStorage>();
        storage.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        storage.GetMarkdownDiskPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), storage);

        // Act
        (Dictionary<string, string> Stubs, bool WroteAll) result = await manager.GenerateStubsAsync(article, writeToDisk: true);

        // Assert
        await Assert.That(result.Stubs.Count).IsEqualTo(ArticleFaker.DefaultLocalizations().Count);
        await Assert.That(result.WroteAll).IsTrue();
        foreach (LocalizationInfo localization in ArticleFaker.DefaultLocalizations()) {
            string path = storage.GetMarkdownDiskPath(localization.Code, article.MarkdownFileName);
            await storage.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }

}
