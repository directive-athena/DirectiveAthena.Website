// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface INoteContentManager : IContentManager<NoteContent> {
    string GetLocalizedTitle(NoteContent article);
    string GetLocalizedSummary(NoteContent article);
    string GetLocalizedFilePath(NoteContent article);
}
