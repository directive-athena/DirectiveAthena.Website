// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DirectiveAthenaWeb.Services.Js;

namespace DirectiveAthenaWebTests.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalizationInitializerTests {
    private static readonly SemaphoreSlim CultureLock = new(1, 1);

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    [SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
    public async Task ApplyPreferredCultureAsync_UsesStoredCulture() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        try {
            // Arrange
            var directiveAthenaWebJs = Substitute.For<IDirectiveAthenaWebJs>();
            directiveAthenaWebJs.GetLocalStorageItemAsync("culture", Arg.Any<CancellationToken>())
                .Returns(new ValueTask<string?>("nl"));

            var logger = Substitute.For<ILogger<LocalizationInitializer>>();
            var providerLogger = Substitute.For<ILogger<LocalizationProvider>>();
            var initializer = new LocalizationInitializer(new LocalizationProvider(providerLogger), directiveAthenaWebJs, logger);

            // Act
            await initializer.ApplyPreferredCultureAsync();

            // Assert
            await Assert.That(CultureInfo.DefaultThreadCurrentUICulture?.TwoLetterISOLanguageName).IsEqualTo("nl");
        }
        finally {
            CultureInfo.DefaultThreadCurrentCulture = previous;
            CultureInfo.DefaultThreadCurrentUICulture = previous;
            CultureLock.Release();
        }
    }

    [Test]
    [SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
    public async Task ApplyPreferredCultureAsync_FallsBackToDefaultWhenUnknown() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        try {
            // Arrange
            var directiveAthenaWebJs = Substitute.For<IDirectiveAthenaWebJs>();
            directiveAthenaWebJs.GetLocalStorageItemAsync("culture", Arg.Any<CancellationToken>())
                .Returns(new ValueTask<string?>("zz"));

            var logger = Substitute.For<ILogger<LocalizationInitializer>>();
            var providerLogger = Substitute.For<ILogger<LocalizationProvider>>();
            var initializer = new LocalizationInitializer(new LocalizationProvider(providerLogger), directiveAthenaWebJs, logger);

            // Act
            await initializer.ApplyPreferredCultureAsync();

            // Assert
            await Assert.That(CultureInfo.DefaultThreadCurrentUICulture?.TwoLetterISOLanguageName).IsEqualTo("en");
        }
        finally {
            CultureInfo.DefaultThreadCurrentCulture = previous;
            CultureInfo.DefaultThreadCurrentUICulture = previous;
            CultureLock.Release();
        }
    }
}
