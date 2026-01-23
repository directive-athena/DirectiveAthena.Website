// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using DirectiveAthena.Website.Services.Localization;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalizationInitializerTests {
    private static readonly SemaphoreSlim CultureLock = new(1, 1);

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task ApplyPreferredCultureAsync_UsesStoredCulture() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        try {
            // Arrange
            var jsRuntime = Substitute.For<Microsoft.JSInterop.IJSRuntime>();
            jsRuntime.InvokeAsync<string>("localStorage.getItem", Arg.Any<object?[]>())
                .Returns(new ValueTask<string>("nl"));

            var initializer = new LocalizationInitializer(new LocalizationProvider(), jsRuntime);
            
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
    public async Task ApplyPreferredCultureAsync_FallsBackToDefaultWhenUnknown() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
        try {
            // Arrange
            var jsRuntime = Substitute.For<Microsoft.JSInterop.IJSRuntime>();
            jsRuntime.InvokeAsync<string>("localStorage.getItem", Arg.Any<object?[]>())
                .Returns(new ValueTask<string>("zz"));

            var initializer = new LocalizationInitializer(new LocalizationProvider(), jsRuntime);

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
