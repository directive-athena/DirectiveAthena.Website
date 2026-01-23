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
public class DevFileSystemPathsTests {
    [Test]
    public async Task GetSharedResxPath_UsesDefaultPathForDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var paths = new DevFileSystemPaths(localizationProvider);

        // Act
        string result = paths.GetSharedResxPath("en");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Shared.resx");
    }

    [Test]
    public async Task GetSharedResxPath_UsesLocalizedPathForNonDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var paths = new DevFileSystemPaths(localizationProvider);

        // Act
        string result = paths.GetSharedResxPath("nl");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Shared.nl.resx");
    }

    [Test]
    public async Task GetTagsResxPath_UsesDefaultPathForDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var paths = new DevFileSystemPaths(localizationProvider);

        // Act
        string result = paths.GetTagsResxPath("en");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Tags.resx");
    }

    [Test]
    public async Task GetTagsResxPath_UsesLocalizedPathForNonDefaultLocale() {
        // Arrange
        var localizationProvider = Substitute.For<ILocalizationProvider>();
        localizationProvider.DefaultLocalization.Returns(new LocalizationInfo("en", "English", "EN", ""));

        var paths = new DevFileSystemPaths(localizationProvider);

        // Act
        string result = paths.GetTagsResxPath("nl");

        // Assert
        await Assert.That(result).IsEqualTo("src/DirectiveAthena.Website/Resources/Tags.nl.resx");
    }
}
