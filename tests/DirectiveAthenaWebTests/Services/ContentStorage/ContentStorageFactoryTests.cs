// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.ContentStorage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ContentStorageFactoryTests {
    private static ContentStorageFactory GetFakeFactory() {
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

        return new ContentStorageFactory(localizationProvider, options, httpClient, loggerFactory);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    [Arguments("writing", "https://cdn.example.com/assets/content/writing/index.json")]
    [Arguments("faq", "https://cdn.example.com/assets/content/faq/index.json")]
    public async Task ForCategory_BuildsIndexPath(string category, string expectedUrl) {
        // Arrange
        ContentStorageFactory factory = GetFakeFactory();
        
        // Act
        IContentStorage storage = factory.ForCategory(category);

        // Assert
        await Assert.That(storage.IndexContentPath).IsEqualTo(expectedUrl);
    }
}
