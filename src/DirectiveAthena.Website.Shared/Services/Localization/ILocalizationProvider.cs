// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;

namespace DirectiveAthena.Website.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface ILocalizationProvider {
    LocalizationInfo DefaultLocalization { get; }
    
    LocalizationInfo GetCurrentLocalization();
    IReadOnlyCollection<LocalizationInfo> GetSupportedLocalizations();
    bool TryGetLocalization(string cultureCode, [NotNullWhen(true)] out LocalizationInfo? config);
    bool IsDefaultCultureCode(string cultureCode);
}
