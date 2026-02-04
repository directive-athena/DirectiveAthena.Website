// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Story;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IStoryContentManager : IContentManager<StoryContent> {
    string GetLocalizedTitle(StoryContent article);
    string GetLocalizedSummary(StoryContent article);
    string GetLocalizedFilePath(StoryContent article);
}
