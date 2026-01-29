// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Story;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IStoryContentManager {
    string GetLocalizedTitle(StoryContent article);
    string GetLocalizedSummary(StoryContent article);
    string GetLocalizedFilePath(StoryContent article);
    Task<string> GetRawMarkdownContentAsync(StoryContent article, string locale, CancellationToken ct = default);

    StoryContent NewWriting();
    bool Validate(IEnumerable<StoryContent> notes, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(StoryContent article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
