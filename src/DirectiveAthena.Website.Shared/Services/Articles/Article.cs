// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class Article : ContentBase {
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public string Date { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public bool Hidden { get; set; }
}
