// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using Bunit;
using DirectiveAthena.Website.Components;
using DirectiveAthena.Website.Services;
using DirectiveAthenaTests.Website.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using System.Reflection;

namespace DirectiveAthenaTests.Website.Components;
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
            ctx.Services.AddMudServices();

            var jsRuntime = new RecordingJsRuntime();
            var nav = new FakeNavigationManager();

            ctx.Services.AddSingleton<IJSRuntime>(jsRuntime);
            ctx.Services.AddSingleton<NavigationManager>(nav);
            ctx.Services.AddSingleton<ILocalizationProvider>(new LocalizationProvider());

            // Act
            _ = ctx.Render<MudPopoverProvider>();
            IRenderedComponent<CultureSelector> cut = ctx.Render<CultureSelector>();

            // Assert
            await Assert.That(cut.Markup).Contains("EN");
            await Assert.That(cut.Markup).Contains("flagcdn.com");

            MethodInfo? changeCulture = typeof(CultureSelector).GetMethod("ChangeCulture", BindingFlags.Instance | BindingFlags.NonPublic);
            await (Task)changeCulture!.Invoke(cut.Instance, ["nl"])!;

            RecordingJsRuntime.Invocation invocation = jsRuntime.Invocations.Single(i => i.Identifier == "localStorage.setItem");
            await Assert.That(invocation.Args).IsNotNull();
            await Assert.That((string)invocation.Args![0]!).IsEqualTo("culture");
            await Assert.That((string)invocation.Args[1]!).IsEqualTo("nl");
            await Assert.That(nav.LastForceLoad).IsTrue();
        }
        finally {
            CultureInfo.CurrentUICulture = previous;
            CultureLock.Release();
        }
    }
}
