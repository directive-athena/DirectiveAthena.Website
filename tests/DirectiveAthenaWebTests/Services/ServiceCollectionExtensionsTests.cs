// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Writings;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Js;
using DirectiveAthenaWeb.Services.Localization;
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
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWritingContentManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWritingContentRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<WritingContent>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<WritingContent>>));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IContactInfoProvider));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IContentStorageFactory));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IDirectiveAthenaWebJs));

        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationInitializer));
        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationProvider));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IFaqContentManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IFaqContentRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<FaqContent>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<FaqContent>>));
    }
}
