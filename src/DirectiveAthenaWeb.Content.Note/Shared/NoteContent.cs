// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;

namespace DirectiveAthenaWeb.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class NoteContent : ContentBase {
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public List<string> Tags { get; set; } = [];
}
