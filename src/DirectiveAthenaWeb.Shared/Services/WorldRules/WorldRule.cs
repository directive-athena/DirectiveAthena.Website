// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRule : ContentBase {
    public Dictionary<string, string> Question { get; set; } = new();
    public Dictionary<string, string> Answer { get; set; } = new();
    public string Date { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public bool ShowOnHomepage { get; set; } = true;
}
