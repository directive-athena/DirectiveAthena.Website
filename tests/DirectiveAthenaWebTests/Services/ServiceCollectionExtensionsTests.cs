// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content.Faq;
using DirectiveAthenaWeb.Content.Note;
using DirectiveAthenaWeb.Services;
using DirectiveAthenaWeb.Services.Contact;
using DirectiveAthenaWeb.Services.Js;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
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
        
        services.AddNoteContent();
        services.AddFaqContent();

        // Assert
        await Assert.That(services).Any(s => s.ServiceType == typeof(INoteContentManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(INoteContentRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<NoteContent>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<NoteContent>>));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IContactInfoProvider));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IR2StorageFactory));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IDirectiveAthenaWebJs));

        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationInitializer));
        await Assert.That(services).Any(s => s.ServiceType == typeof(ILocalizationProvider));

        await Assert.That(services).Any(s => s.ServiceType == typeof(IFaqContentManager));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IFaqContentRepository));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<FaqContent>));
        await Assert.That(services).Any(s => s.ServiceType == typeof(IValidator<IEnumerable<FaqContent>>));
    }
}
