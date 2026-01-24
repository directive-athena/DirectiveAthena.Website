// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services;
using DirectiveAthena.Website.Services.Articles;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaTests.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ServiceCollectionExtensionsTests {
    [Test]
    public async Task AddWebsiteServices_RegistersCoreServices() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWebsiteServices();

        // Assert
        await Assert.That(services.Any(s => s.ServiceType == typeof(IArticleManager))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(IArticleRepository))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(ILocalFileStorage))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(IContentStorageFactory))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(IResourceStorage))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(ILocalizationProvider))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(ILocalizationInitializer))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(IValidator<Article>))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(IValidator<IEnumerable<Article>>))).IsTrue();
    }
}
