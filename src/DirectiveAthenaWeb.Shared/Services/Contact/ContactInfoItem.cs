// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Contact;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ContactInfoItem {
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string SimpleIconsUrl { get; init; } = string.Empty;
    public bool IncludeInFooter { get; init; } = IncludeInFooterDefault;
    
    public const bool IncludeInFooterDefault = true;
}
