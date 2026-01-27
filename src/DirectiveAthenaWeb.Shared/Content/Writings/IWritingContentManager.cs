// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Writings;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWritingContentManager {
    string GetLocalizedTitle(WritingContent article);
    string GetLocalizedSummary(WritingContent article);
    string GetLocalizedFilePath(WritingContent article);
    Task<string> GetRawMarkdownContentAsync(WritingContent article, string locale, CancellationToken ct = default);

    WritingContent NewWriting();
    bool Validate(IEnumerable<WritingContent> writings, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(WritingContent article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
