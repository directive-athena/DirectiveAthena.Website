// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using Bunit;
using DirectiveAthenaWeb.Components;
using DirectiveAthenaWeb.Services.Js;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWebTests.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using System.Reflection;

namespace DirectiveAthenaWebTests.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class CultureSelectorTests {
    private static readonly SemaphoreSlim CultureLock = new(1, 1);

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task CultureSelector_RendersCurrentCultureAndInvokesChange() {
        await CultureLock.WaitAsync();
        CultureInfo previous = CultureInfo.CurrentUICulture;
        try {
            // Arrange
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            await using var ctx = new BunitContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            ctx.Services.AddMudServices();

            var webJs = Substitute.For<IDirectiveAthenaWebJs>();
            var nav = new FakeNavigationManager();

            ctx.Services.AddSingleton(webJs);
            ctx.Services.AddSingleton<NavigationManager>(nav);
            var logger = Substitute.For<ILogger<LocalizationProvider>>();
            ctx.Services.AddSingleton<ILocalizationProvider>(new LocalizationProvider(logger, BuildOptions()));

            // Act
            _ = ctx.Render<MudPopoverProvider>();
            IRenderedComponent<CultureSelector> cut = ctx.Render<CultureSelector>();

            // Assert
            await Assert.That(cut.Markup).Contains("EN");
            await Assert.That(cut.Markup).Contains("flagcdn.com");

            MethodInfo? changeCulture = typeof(CultureSelector).GetMethod("ChangeCulture", BindingFlags.Instance | BindingFlags.NonPublic);
            await (Task)changeCulture!.Invoke(cut.Instance, ["nl"])!;

            await webJs.Received(1).SetLocalStorageItemAsync("culture", "nl", Arg.Any<CancellationToken>());
            await Assert.That(nav.LastForceLoad).IsTrue();
        }
        finally {
            CultureInfo.CurrentUICulture = previous;
            CultureLock.Release();
        }
    }

    private static IOptions<LocalizationOptions> BuildOptions()
        => TestLocalization.CreateLocalizationOptions();
}
