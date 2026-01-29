// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;

namespace DirectiveAthenaWeb.Content.Story;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class StoryContent : ContentBase {
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public List<string> Chapters { get; set; } = new();
    public List<string> Tags { get; set; } = [];
}
