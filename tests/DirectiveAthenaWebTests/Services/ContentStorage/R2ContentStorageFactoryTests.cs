// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.ContentStorage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2ContentStorageFactoryTests {
    private static R2ContentStorageFactory GetFakeFactory() {
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        IOptions<R2StorageOptions> options = Options.Create(new R2StorageOptions {
            PublicBaseUrl = "https://cdn.example.com/",
            AccountId = "account",
            BucketName = "bucket"
        });

        var httpClient = new HttpClient();
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new R2ContentStorageFactory(localizationProvider, options, httpClient, environment, loggerFactory);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -------------------1`----------------------------------------------------------------------------------------------
    [Test]
    [Arguments("faq", "https://cdn.example.com/content/faq/index.json")]
    [Arguments("note", "https://cdn.example.com/content/note/index.json")]
    [Arguments("story", "https://cdn.example.com/content/story/index.json")]
    public async Task ForCategory_BuildsIndexPath(string category, string expectedUrl) {
        // Arrange
        R2ContentStorageFactory factory = GetFakeFactory();
        
        // Act
        IContentStorage storage = factory.ForCategory(category);

        // Assert
        await Assert.That(storage.IndexContentPath).IsEqualTo(expectedUrl);
    }
}
