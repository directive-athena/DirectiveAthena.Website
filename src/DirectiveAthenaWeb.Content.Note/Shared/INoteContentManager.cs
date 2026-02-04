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
    
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(NoteContent article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
