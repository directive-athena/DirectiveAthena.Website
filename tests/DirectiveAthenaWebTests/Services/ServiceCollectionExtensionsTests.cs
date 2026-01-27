// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Writings;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Js;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.WorldFaq;
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
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWritingManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWritingRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<Writing>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<Writing>>));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IContactInfoProvider));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IContentStorageFactory));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IDirectiveAthenaWebJs));

        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationInitializer));
        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationProvider));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IWorldFaqManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IWorldFaqRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<DirectiveAthenaWeb.Services.WorldFaq.WorldFaq>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<DirectiveAthenaWeb.Services.WorldFaq.WorldFaq>>));
    }
}
