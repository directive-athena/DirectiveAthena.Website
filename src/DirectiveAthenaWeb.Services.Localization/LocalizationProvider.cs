// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectiveAthenaWeb.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<ILocalizationProvider>]
public class LocalizationProvider(ILogger<LocalizationProvider> logger, IOptions<LocalizationOptions> optionsAccessor) : ILocalizationProvider {
    private readonly ImmutableArray<LocalizationInfo> _supportedLocalizations = BuildLocalizations(optionsAccessor.Value);

    public LocalizationInfo DefaultLocalization => _supportedLocalizations[0];

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public LocalizationInfo GetCurrentLocalization() {
        string code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        LocalizationInfo? localization = _supportedLocalizations.FirstOrDefault(c => c.Code == code);
        if (localization is not null) return localization;

        logger.Debug("Falling back to default culture for {Culture}.", code);
        return DefaultLocalization;
    }

    public IReadOnlyCollection<LocalizationInfo> GetSupportedLocalizations()
        => _supportedLocalizations;

    public bool TryGetLocalization(string cultureCode, [NotNullWhen(true)] out LocalizationInfo? config) {
        config = _supportedLocalizations.FirstOrDefault(c => c.Code == cultureCode);
        if (config is null) logger.Debug("Unknown culture code requested: {Culture}.", cultureCode);

        return config is not null;
    }

    public bool IsDefaultCultureCode(string cultureCode)
        => cultureCode == DefaultLocalization.Code;

    private static ImmutableArray<LocalizationInfo> BuildLocalizations(LocalizationOptions options) {
        if (options.Items.Count == 0) return ImmutableArray<LocalizationInfo>.Empty;
        return options.Items
            .Select(item => new LocalizationInfo(item.Code, item.DisplayName, item.Abbreviation, item.FlagPath))
            .ToImmutableArray();
    }
}
