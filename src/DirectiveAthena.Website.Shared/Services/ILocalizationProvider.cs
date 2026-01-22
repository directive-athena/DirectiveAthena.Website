// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;
using System.Diagnostics.CodeAnalysis;

namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface ILocalizationProvider {
    LocalizationInfo GetCurrentLocalization();
    IReadOnlyCollection<LocalizationInfo> GetSupportedLocalizations();
    bool TryGetLocalization(string cultureCode, [NotNullWhen(true)] out LocalizationInfo? config);
    bool IsDefaultCultureCode(string cultureCode);
}
