// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<ILocalizationProvider>]
public class LocalizationProvider(ILogger<LocalizationProvider> logger) : ILocalizationProvider {
    public static readonly LocalizationInfo[] SupportedLocalizations = [
        new("en", "English", "EN", "https://flagcdn.com/w40/us.png"),
        new("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")
    ];

    public LocalizationInfo DefaultLocalization => SupportedLocalizations.First();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public LocalizationInfo GetCurrentLocalization() {
        string code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        LocalizationInfo? localization = SupportedLocalizations.FirstOrDefault(c => c.Code == code);
        if (localization is not null) return localization;

        logger.Debug("Falling back to default culture for {Culture}.", code);
        return DefaultLocalization;
    }

    public IReadOnlyCollection<LocalizationInfo> GetSupportedLocalizations()
        => SupportedLocalizations.AsReadOnly();

    public bool TryGetLocalization(string cultureCode, [NotNullWhen(true)] out LocalizationInfo? config) {
        config = SupportedLocalizations.FirstOrDefault(c => c.Code == cultureCode);
        if (config is null) logger.Debug("Unknown culture code requested: {Culture}.", cultureCode);

        return config is not null;
    }

    public bool IsDefaultCultureCode(string cultureCode)
        => cultureCode == DefaultLocalization.Code;
}
