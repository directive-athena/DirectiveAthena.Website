// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Localization;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalizationOptions {
    public List<LocalizationItem> Items { get; set; } = [];

    public LocalizationOptions AddLocalization(
        string code,
        string displayName,
        string abbreviation,
        string flagPath
    ) {
        Items.Add(new LocalizationItem {
            Code = code,
            DisplayName = displayName,
            Abbreviation = abbreviation,
            FlagPath = flagPath
        });
        return this;
    }
}
