// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Services;

namespace DirectiveAthenaTests.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalizationProviderTests {
    private static readonly SemaphoreSlim CultureLock = new(1, 1);

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task GetCurrentLocalization_UsesCurrentUiCulture() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.CurrentUICulture;
        try {
            // Arrange
            CultureInfo.CurrentUICulture = new CultureInfo("nl");
            var provider = new LocalizationProvider();
            
            // Act
            LocalizationInfo result = provider.GetCurrentLocalization();

            // Assert
            await Assert.That(result.Code).IsEqualTo("nl");
        }
        finally {
            CultureInfo.CurrentUICulture = previous;
            CultureLock.Release();
        }
    }

    [Test]
    public async Task TryGetLocalization_ReturnsExpectedResults() {
        // Arrange
        var provider = new LocalizationProvider();

        // Act
        bool found = provider.TryGetLocalization("en", out LocalizationInfo? en);
        bool missing = provider.TryGetLocalization("zz", out LocalizationInfo? none);

        // Assert
        await Assert.That(found).IsTrue();
        await Assert.That(en?.Code).IsEqualTo("en");
        await Assert.That(missing).IsFalse();
        await Assert.That(none).IsNull();
    }

    [Test]
    public async Task IsDefaultCultureCode_MatchesDefault() {
        // Arrange
        
        // Act
        var provider = new LocalizationProvider();

        // Assert
        await Assert.That(provider.IsDefaultCultureCode("en")).IsTrue();
        await Assert.That(provider.IsDefaultCultureCode("nl")).IsFalse();
    }
}
