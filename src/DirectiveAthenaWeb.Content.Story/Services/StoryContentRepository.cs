// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IStoryContentRepository>]
[InjectableScoped<IContentRepository<StoryContent>>]
internal class StoryContentRepository(
    [FromKeyedServices("story")] IContentStorage storage,
    ILogger<StoryContentRepository> logger
) : ContentRepositoryBase<StoryContent>(storage, logger), IStoryContentRepository;
