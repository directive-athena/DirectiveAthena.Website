// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
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
        localizationProvider.DefaultLocalization.Returns(localizations.First(l => l.Code == "en"));
        localizationProvider.GetCurrentLocalization().Returns(localizations.First(l => l.Code == currentCode));
        localizationProvider.GetSupportedLocalizations().Returns(localizations);
        return localizationProvider;
    }

    private static ArticleManager CreateManager(
        ILocalizationProvider localizationProvider,
        IDevFileSystemManager? devFs = null,
        HttpClient? http = null,
        IDevFileSystemPaths? devFsPaths = null
    ) {
        devFs ??= Substitute.For<IDevFileSystemManager>();
        http ??= new HttpClient();
        devFsPaths ??= Substitute.For<IDevFileSystemPaths>();
        var validator = new ArticleCollectionValidator(new ArticleValidator(localizationProvider));
        return new ArticleManager(localizationProvider, devFs, http, validator, devFsPaths);
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
        ArticleManager manager = CreateManager(localizationProvider);
        
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
        await Assert.That(article.Id).IsNotNullOrWhiteSpace();
        await Assert.That(article.File).IsNotNullOrWhiteSpace();
    }

    [Test]
    public async Task Validate_RejectsMissingIdOrFile() {
        // Arrange
        Article[] articles = [
            new() { Id = "", File = "missing.md" }
        ];

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));
        
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
    public async Task Validate_RejectsDuplicateFiles() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(150),
            ArticleFaker.Create(151)
        ];
        articles[1].File = articles[0].File;

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"));

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
        var devFs = Substitute.For<IDevFileSystemManager>();
        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), devFs);
        
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

        var devFsPaths = Substitute.For<IDevFileSystemPaths>();
        devFsPaths
            .GetMarkdownPath(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => $"{call.ArgAt<string>(0)}/{call.ArgAt<string>(1)}");

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), devFs, devFsPaths: devFsPaths);

        // Act
        Dictionary<string, string> stubs = await manager.GenerateStubsAsync(article, writeToDisk: true);

        // Assert
        await Assert.That(stubs.Count).IsEqualTo(ArticleFaker.DefaultLocalizations().Count);
        foreach (LocalizationInfo localization in ArticleFaker.DefaultLocalizations()) {
            string path = devFsPaths.GetMarkdownPath(localization.Code, article.File);
            await devFs.Received(1).WriteFileAsync(path, Arg.Any<string>());
        }
    }

    [Test]
    public async Task EnsureResxAsync_CreatesMissingResxFiles() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(true));

        var devFsPaths = Substitute.For<IDevFileSystemPaths>();
        devFsPaths.GetSharedResxPath("en").Returns("shared.en.resx");
        devFsPaths.GetSharedResxPath("nl").Returns("shared.nl.resx");

        string enPath = devFsPaths.GetSharedResxPath("en");
        string nlPath = devFsPaths.GetSharedResxPath("nl");

        devFs.ReadFileAsync(enPath).Returns(new ValueTask<string?>("existing"));
        devFs.ReadFileAsync(nlPath).Returns(new ValueTask<string?>());
        devFs.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        
        // Act
        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), devFs, devFsPaths: devFsPaths);
        await manager.EnsureResxAsync();

        // Assert
        await devFs.DidNotReceive().WriteFileAsync(enPath, Arg.Any<string>());
        await devFs.Received(1).WriteFileAsync(nlPath, Arg.Any<string>());
    }

    [Test]
    public async Task EnsureResxAsync_DoesNotReadOrWriteWhenNotLocalhost() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(false);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(true));

        var devFsPaths = Substitute.For<IDevFileSystemPaths>();
        devFsPaths.GetSharedResxPath(Arg.Any<string>()).Returns("shared.resx");

        ArticleManager manager = CreateManager(CreateLocalizationProvider("en"), devFs, devFsPaths: devFsPaths);

        // Act
        await manager.EnsureResxAsync();

        // Assert
        await devFs.DidNotReceiveWithAnyArgs().ReadFileAsync(null!);
        await devFs.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }
}
