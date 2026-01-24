// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ResourceStorageTests {
    [Test]
    public async Task GetSharedResxPath_UsesDefaultPathForDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var storage = new ResourceStorage(Substitute.For<ILocalFileStorage>(), localizationProvider);

        // Act
        string result = storage.GetSharedResxPath("en");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Shared.resx");
    }

    [Test]
    public async Task GetSharedResxPath_UsesLocalizedPathForNonDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var storage = new ResourceStorage(Substitute.For<ILocalFileStorage>(), localizationProvider);

        // Act
        string result = storage.GetSharedResxPath("nl");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Shared.nl.resx");
    }

    [Test]
    public async Task GetTagsResxPath_UsesDefaultPathForDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var storage = new ResourceStorage(Substitute.For<ILocalFileStorage>(), localizationProvider);

        // Act
        string result = storage.GetTagsResxPath("en");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Tags.resx");
    }

    [Test]
    public async Task GetTagsResxPath_UsesLocalizedPathForNonDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var storage = new ResourceStorage(Substitute.For<ILocalFileStorage>(), localizationProvider);

        // Act
        string result = storage.GetTagsResxPath("nl");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Tags.nl.resx");
    }
}
