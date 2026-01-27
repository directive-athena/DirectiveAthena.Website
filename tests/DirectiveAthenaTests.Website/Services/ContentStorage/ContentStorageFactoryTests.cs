// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.ContentStorage;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services.ContentStorage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ContentStorageFactoryTests {
    [Test]
    public async Task ForCategory_BuildsArticleIndexPath() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        IOptions<R2StorageOptions> options = Options.Create(new R2StorageOptions {
            PublicBaseUrl = "https://cdn.example.com/assets",
            AccountId = "account",
            BucketName = "bucket"
        });

        var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

        var factory = new ContentStorageFactory(localizationProvider, options, httpClient, loggerFactory);

        // Act
        IContentStorage storage = factory.ForCategory(ContentCategory.Articles);

        // Assert
        await Assert.That(storage.IndexContentPath).IsEqualTo("https://cdn.example.com/assets/content/articles/index.json");
    }

    [Test]
    public async Task ForCategory_BuildsWorldRulesIndexPath() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        IOptions<R2StorageOptions> options = Options.Create(new R2StorageOptions {
            PublicBaseUrl = "https://cdn.example.com/assets/",
            AccountId = "account",
            BucketName = "bucket"
        });

        var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

        var factory = new ContentStorageFactory(localizationProvider, options, httpClient, loggerFactory);

        // Act
        IContentStorage storage = factory.ForCategory(ContentCategory.WorldRules);

        // Assert
        await Assert.That(storage.IndexContentPath).IsEqualTo("https://cdn.example.com/assets/content/world-rules/index.json");
    }
}
