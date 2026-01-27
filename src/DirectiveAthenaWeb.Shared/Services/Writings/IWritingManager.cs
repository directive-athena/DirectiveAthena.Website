// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Writings;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWritingManager {
    string GetLocalizedTitle(Writing article);
    string GetLocalizedSummary(Writing article);
    string GetLocalizedFilePath(Writing article);
    Task<string> GetRawMarkdownContentAsync(Writing article, string locale, CancellationToken ct = default);

    Writing NewWriting();
    bool Validate(IEnumerable<Writing> writings, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(Writing article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
