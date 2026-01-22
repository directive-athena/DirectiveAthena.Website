// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Models;
using Microsoft.JSInterop;
using System.Globalization;

namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<ILocalizationInitializer>]
public class LocalizationInitializer(ILocalizationProvider localizationProvider, IJSRuntime jsRuntime) : ILocalizationInitializer {

    public async Task ApplyPreferredCultureAsync() {
        string result = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "culture");
        if (!localizationProvider.TryGetLocalization(result, out LocalizationInfo? culture)) {
            culture = LocalizationProvider.DefaultLocalization;
        }

        var cultureInfo = new CultureInfo(culture.Code);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
