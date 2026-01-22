// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.Validation;
using DirectiveAthenaTests.Website.Helpers;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services;
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
        localizationProvider.GetCurrentLocalization().Returns(localizations.First(l => l.Code == currentCode));
        localizationProvider.GetSupportedLocalizations().Returns(localizations);
        return localizationProvider;
    }

    private static ArticleManager CreateManager(
        ILocalizationProvider localizationProvider,
        IDevFileSystemManager? devFs = null,
        HttpClient? http = null
    ) {
        devFs ??= Substitute.For<IDevFileSystemManager>();
        http ??= new HttpClient();
        var validator = new ArticleCollectionValidator(new ArticleValidator(localizationProvider));
        return new ArticleManager(localizationProvider, devFs, http, validator);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetLocalizedTitle_FallsBackToDefaultCulture() {
        // Arrange
        Article article = ArticleFaker.Create(100, includeNl: false);
        ILocalizationProvider localizationProvider = CreateLocalizationProvider("nl");
        var manager = CreateManager(localizationProvider);

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
        var manager = CreateManager(localizationProvider);
        
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
        var manager = CreateManager(localizationProvider);
        
        // Act
        string result = manager.GetLocalizedFilePath(article);

        // Assert
        await Assert.That(result).IsEqualTo($"content/articles/nl/{article.File}");
    }

    [Test]
    public async Task GetRawMarkdownContentAsync_ReturnsEmptyOnFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var manager = CreateManager(CreateLocalizationProvider("en"), http: http);
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
        var manager = CreateManager(localizationProvider);
        
        // Act
        Article article = manager.NewArticle();
        HashSet<string> expected = ArticleFaker.DefaultLocalizations().Select(c => c.Code).ToHashSet();

        // Assert
        await Assert.That(article.Title.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Summary.Keys.ToHashSet()).IsEquivalentTo(expected);
        await Assert.That(article.Id).IsNotNullOrWhiteSpace();
        await Assert.That(article.File).IsNotNullOrWhiteSpace();
    }

    [Test]
    public async Task Validate_RejectsMissingIdOrFile() {
        // Arrange
        Article[] articles = [
            new() { Id = "", File = "missing.md" }
        ];

        var manager = CreateManager(CreateLocalizationProvider("en"));
        
        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Some posts have missing Id or File!");
    }

    [Test]
    public async Task Validate_RejectsDuplicateIds() {
        // Arrange
        Article[] articles = [
            new() { Id = "dup", File = "a.md" },
            new() { Id = "dup", File = "b.md" }
        ];

        var manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate IDs found!");
    }

    [Test]
    public async Task Validate_RejectsDuplicateFiles() {
        // Arrange
        Article[] articles = [
            new() { Id = "a", File = "dup.md" },
            new() { Id = "b", File = "dup.md" }
        ];

        var manager = CreateManager(CreateLocalizationProvider("en"));

        // Act
        bool result = manager.Validate(articles, out string? error);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(error).IsEqualTo("Duplicate files found!");
    }

    [Test]
    public async Task Validate_RejectsMissingLocalizedTitles() {
        // Arrange
        Article article = ArticleFaker.Create(200, includeNl: false);
        Article[] articles = [article];

        var manager = CreateManager(CreateLocalizationProvider("en"));

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

        var manager = CreateManager(CreateLocalizationProvider("en"));

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
        var devFs = Substitute.For<IDevFileSystemManager>();
        var manager = CreateManager(CreateLocalizationProvider("en"), devFs);
        
        // Act
        Dictionary<string, string> stubs = await manager.GenerateStubsAsync(article, writeToDisk: false);

        // Assert
        await Assert.That(stubs.Count).IsEqualTo(ArticleFaker.DefaultLocalizations().Count);
        await devFs.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task GenerateStubsAsync_WritesWhenLocalhostAndPermitted() {
        // Arrange
        Article article = ArticleFaker.Create(121);
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(true));
        devFs.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        devFs.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var manager = CreateManager(CreateLocalizationProvider("en"), devFs);

        // Act
        Dictionary<string, string> stubs = await manager.GenerateStubsAsync(article, writeToDisk: true);

        // Assert
        await Assert.That(stubs.Count).IsEqualTo(ArticleFaker.DefaultLocalizations().Count);
        foreach (LocalizationInfo localization in ArticleFaker.DefaultLocalizations()) {
            string path = DevFileSystemPaths.GetMarkdownPath(localization.Code, article.File);
            await devFs.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }

    [Test]
    public async Task EnsureResxAsync_CreatesMissingResxFiles() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(true));

        string enPath = DevFileSystemPaths.GetSharedResxPath("en");
        string nlPath = DevFileSystemPaths.GetSharedResxPath("nl");

        devFs.ReadFileAsync(enPath).Returns(new ValueTask<string?>("existing"));
        devFs.ReadFileAsync(nlPath).Returns(new ValueTask<string?>());
        devFs.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        
        // Act
        var manager = CreateManager(CreateLocalizationProvider("en"), devFs);
        await manager.EnsureResxAsync();

        // Assert
        await devFs.DidNotReceive().WriteFileAsync(enPath, Arg.Any<string>());
        await devFs.Received(1).WriteFileAsync(nlPath, Arg.Any<string>());
    }
}
