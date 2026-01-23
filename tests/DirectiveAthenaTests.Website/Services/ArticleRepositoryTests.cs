// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net;
using System.Text;
using System.Text.Json;
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using DirectiveAthenaTests.Website.Helpers;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ArticleRepositoryTests {
    [Test]
    public async Task GetPostsAsync_FiltersOutHiddenPosts() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(1, hidden: false),
            ArticleFaker.Create(2, hidden: true),
            ArticleFaker.Create(3, hidden: false)
        ];

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(articles), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new ArticleRepository(http, Substitute.For<IDevFileSystemManager>(), Substitute.For<ILocalizationProvider>(), Substitute.For<IDevFileSystemPaths>());

        // Act
        List<Article> result = (await repo.GetPostsAsync()).ToList();

        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(p => p.Hidden)).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_HandlesHttpClientFailure() {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => throw new HttpRequestException("boom"));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new ArticleRepository(http, Substitute.For<IDevFileSystemManager>(), Substitute.For<ILocalizationProvider>(), Substitute.For<IDevFileSystemPaths>());

        // Act
        IEnumerable<Article> result = await repo.GetPostsAsync();

        // Assert
        await Assert.That(result.Any()).IsFalse();
    }

    [Test]
    public async Task GetPostsAsync_CachesResults() {
        // Arrange
        Article[] articles = [
            ArticleFaker.Create(10, hidden: false),
            ArticleFaker.Create(11, hidden: true)
        ];

        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(JsonSerializer.Serialize(articles), Encoding.UTF8, "application/json")
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var repo = new ArticleRepository(http, Substitute.For<IDevFileSystemManager>(), Substitute.For<ILocalizationProvider>(), Substitute.For<IDevFileSystemPaths>());
        
        // Act
        _ = (await repo.GetPostsAsync()).ToList();
        _ = (await repo.GetPostsAsync(includeHidden: true)).ToList();

        // Assert
        await Assert.That(handler.CallCount).IsEqualTo(1);
    }

    [Test]
    public async Task SaveAsync_RespectsLocalhostAndPermissions() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        devFs.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        var devFsPaths = Substitute.For<IDevFileSystemPaths>();
        devFsPaths.GetIndexPath().Returns("index.json");

        var repo = new ArticleRepository(new HttpClient(), devFs, Substitute.For<ILocalizationProvider>(), devFsPaths);
        Article[] articles = [ArticleFaker.Create(21)];
        
        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsTrue();
        await devFs.Received(1).WriteFileAsync(devFsPaths.GetIndexPath(), Arg.Any<string>());
    }

    [Test]
    public async Task SaveAsync_ReturnsFalseWhenNotLocalhost() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(false);

        var repo = new ArticleRepository(new HttpClient(), devFs, Substitute.For<ILocalizationProvider>(), Substitute.For<IDevFileSystemPaths>());
        Article[] articles = [ArticleFaker.Create(22)];

        // Act
        bool result = await repo.SaveAsync(articles);

        // Assert
        await Assert.That(result).IsFalse();
        await devFs.DidNotReceiveWithAnyArgs().WriteFileAsync(null!, null!);
    }

    [Test]
    public async Task DeleteAsync_DeletesLocalizedFilesAndSaves() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(true));
        devFs.VerifyPermissionAsync().Returns(new ValueTask<bool>(true));
        devFs.DeleteFileAsync(Arg.Any<string>()).Returns(new ValueTask<bool>(true));
        devFs.WriteFileAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(new ValueTask<bool>(true));

        LocalizationInfo[] localizations = [
            new("en", "English", "EN", ""),
            new("nl", "Nederlands", "NL", "")
        ];

        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.GetSupportedLocalizations().Returns(localizations);

        Article article = ArticleFaker.Create(30);
        var devFsPaths = Substitute.For<IDevFileSystemPaths>();
        devFsPaths.GetIndexPath().Returns("index.json");
        devFsPaths.GetMarkdownPath("en", article.File).Returns($"en/{article.File}");
        devFsPaths.GetMarkdownPath("nl", article.File).Returns($"nl/{article.File}");

        var repo = new ArticleRepository(new HttpClient(), devFs, localizationProvider, devFsPaths);
        Article[] articles = [article];
        
        // Act
        bool result = await repo.DeleteAsync(article, articles);

        // Assert
        await Assert.That(result).IsTrue();
        foreach (LocalizationInfo localization in localizations) {
            string path = devFsPaths.GetMarkdownPath(localization.Code, article.File);
            await devFs.Received(1).DeleteFileAsync(path);
        }
        await devFs.Received(1).WriteFileAsync(devFsPaths.GetIndexPath(), Arg.Any<string>());
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalseWhenNoAccess() {
        // Arrange
        var devFs = Substitute.For<IDevFileSystemManager>();
        devFs.IsLocalhost.Returns(true);
        devFs.HasAccessAsync().Returns(new ValueTask<bool>(false));

        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.GetSupportedLocalizations().Returns(new[] { new LocalizationInfo("en", "English", "EN", "") });

        var repo = new ArticleRepository(new HttpClient(), devFs, localizationProvider, Substitute.For<IDevFileSystemPaths>());
        Article article = ArticleFaker.Create(40);
        Article[] articles = [article];

        // Act
        bool result = await repo.DeleteAsync(article, articles);

        // Assert
        await Assert.That(result).IsFalse();
        await devFs.DidNotReceiveWithAnyArgs().DeleteFileAsync(null!);
    }
}
