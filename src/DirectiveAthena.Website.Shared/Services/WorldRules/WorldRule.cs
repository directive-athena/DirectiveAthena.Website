// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class WorldRule {
    public string Id { get; set; } = "";
    public Dictionary<string, string> Question { get; set; } = new();
    public Dictionary<string, string> Answer { get; set; } = new();
    public string Date { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string File { get; set; } = "";
}
