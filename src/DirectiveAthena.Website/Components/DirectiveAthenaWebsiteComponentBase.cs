// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Services;
using Microsoft.AspNetCore.Components;

namespace DirectiveAthena.Website.Components;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class DirectiveAthenaWebsiteComponentBase : ComponentBase {
    [Inject] public ILocalizationProvider LocalizationProvider { get; set; } = null!;
    
    protected LocalizationInfo CurrentCulture => LocalizationProvider.GetCurrentLocalization();
}
