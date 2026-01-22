// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Models;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class Article {
    public string Id { get; set; } = "";
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public string Date { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string File { get; set; } = "";
    public bool Hidden { get; set; }
}