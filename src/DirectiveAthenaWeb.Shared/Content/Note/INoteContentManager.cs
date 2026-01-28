// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Note;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface INoteContentManager {
    string GetLocalizedTitle(NoteContent article);
    string GetLocalizedSummary(NoteContent article);
    string GetLocalizedFilePath(NoteContent article);
    Task<string> GetRawMarkdownContentAsync(NoteContent article, string locale, CancellationToken ct = default);

    NoteContent NewWriting();
    bool Validate(IEnumerable<NoteContent> notes, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(NoteContent article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
