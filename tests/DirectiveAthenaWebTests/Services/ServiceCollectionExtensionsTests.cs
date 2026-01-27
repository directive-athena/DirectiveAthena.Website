// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Articles;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Js;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.WorldRules;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectiveAthenaWebTests.Services;
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
        await Assert.That(services).Any(s => s.ServiceType == typeof(IArticleManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IArticleRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<Article>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<Article>>));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IContactInfoProvider));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IContentStorageFactory));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IDirectiveAthenaWebJs));

        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationInitializer));
        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationProvider));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IWorldRuleManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWorldRuleRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<WorldRule>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<WorldRule>>));
    }
}
