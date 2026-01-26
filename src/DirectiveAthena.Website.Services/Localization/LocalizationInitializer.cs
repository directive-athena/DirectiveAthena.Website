// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<ILocalizationInitializer>]
public class LocalizationInitializer(
    ILocalizationProvider localizationProvider,
    IJSRuntime jsRuntime,
    ILogger<LocalizationInitializer> logger
) : ILocalizationInitializer {

    public async Task ApplyPreferredCultureAsync() {
        string? result = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "culture");
        if (string.IsNullOrWhiteSpace(result) || !localizationProvider.TryGetLocalization(result, out LocalizationInfo? culture)) {
            logger.Information("No stored culture found; using default {Culture}.", localizationProvider.DefaultLocalization.Code);
            culture = localizationProvider.DefaultLocalization;
        }
        else {
            logger.Information("Loaded preferred culture {Culture}.", culture.Code);
        }

        var cultureInfo = new CultureInfo(culture.Code);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}
