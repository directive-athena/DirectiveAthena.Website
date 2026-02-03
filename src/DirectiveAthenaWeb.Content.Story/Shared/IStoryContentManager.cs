// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;

namespace DirectiveAthenaWeb.Content.Story;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IStoryContentManager : IContentManager<StoryContent> {
    string GetLocalizedTitle(StoryContent article);
    string GetLocalizedSummary(StoryContent article);
    string GetLocalizedFilePath(StoryContent article);

    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(StoryContent article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
