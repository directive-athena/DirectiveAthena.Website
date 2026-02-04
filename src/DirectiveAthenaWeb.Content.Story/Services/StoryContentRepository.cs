// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization.Metadata;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IStoryContentRepository>]
[InjectableScoped<IContentRepository<StoryContent>>]
internal class StoryContentRepository(
    IR2Storage<StoryContent> storage,
    ILogger<StoryContentRepository> logger
) : ContentRepositoryBase<StoryContent>(storage, logger), IStoryContentRepository {
    protected override JsonTypeInfo<StoryContent[]> ContentListTypeInfo
        => StoryContentJsonContext.Default.StoryContentArray;
}
