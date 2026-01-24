// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Localization;
using Microsoft.AspNetCore.Components;

namespace DirectiveAthena.Website.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WebsiteComponentBase : ComponentBase {
    [Inject] public ILocalizationProvider LocalizationProvider { get; set; } = null!;
    [Inject] public ILogger<WebsiteComponentBase> Logger { get; set; } = null!;
    
    protected LocalizationInfo CurrentCulture => LocalizationProvider.GetCurrentLocalization();
}
