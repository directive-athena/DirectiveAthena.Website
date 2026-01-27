// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Components;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WebsiteComponentBase : ComponentBase {
    [Inject] public ILocalizationProvider LocalizationProvider { get; set; } = null!;
    [Inject] public ILogger<WebsiteComponentBase> Logger { get; set; } = null!;
    
    protected LocalizationInfo CurrentCulture => LocalizationProvider.GetCurrentLocalization();
}
