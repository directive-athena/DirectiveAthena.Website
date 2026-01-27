// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.Extensions.Logging;
using DirectiveAthenaWeb.Services.Js;

namespace DirectiveAthenaWeb.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<ILocalizationInitializer>]
public class LocalizationInitializer(
    ILocalizationProvider localizationProvider,
    IDirectiveAthenaWebJs webJs,
    ILogger<LocalizationInitializer> logger
) : ILocalizationInitializer {

    public async Task ApplyPreferredCultureAsync() {
        string? result = await webJs.GetLocalStorageItemAsync("culture");
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
