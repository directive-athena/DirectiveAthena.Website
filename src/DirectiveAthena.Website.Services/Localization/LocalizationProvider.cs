// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Models;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DirectiveAthena.Website.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<ILocalizationProvider>]
public class LocalizationProvider : ILocalizationProvider {
    public static readonly LocalizationInfo[] SupportedLocalizations = [
        new("en", "English", "EN", "https://flagcdn.com/w40/us.png"),
        new("nl", "Nederlands", "NL", "https://flagcdn.com/w40/nl.png")
    ];

    public static LocalizationInfo DefaultLocalization => SupportedLocalizations.First();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public LocalizationInfo GetCurrentLocalization() 
        => SupportedLocalizations.FirstOrDefault(c => c.Code == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ?? DefaultLocalization;
    
    public IReadOnlyCollection<LocalizationInfo> GetSupportedLocalizations() 
        => SupportedLocalizations.AsReadOnly();
    
    public bool TryGetLocalization(string cultureCode, [NotNullWhen(true)] out LocalizationInfo? config) {
        config = SupportedLocalizations.FirstOrDefault(c => c.Code == cultureCode);
        return config is not null;
    }
    
    public bool IsDefaultCultureCode(string cultureCode)
        => cultureCode == DefaultLocalization.Code;
}
