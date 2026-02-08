// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IStoryContentManager>]
[InjectableScoped<IContentManager<StoryContent>>]
internal class StoryContentManager(
    ILocalizationProvider localizationProvider,
    IR2Storage<StoryContent> storage,
    ILogger<ContentManagerBase<StoryContent>> logger
) : ContentManagerBase<StoryContent>(localizationProvider, storage, logger), IStoryContentManager {

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public string GetLocalizedFilePath(StoryContent article) {
        LocalizationInfo localization = LocalizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }

    public override StoryContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = LocalizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Story");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new StoryContent {
            Id = id,
            Title = titles,
            Author = "Anna Sas",
            Summary = summaries,
            Tags = [
            ],
            CreatedAt = now,
            LastModifiedAt = now,
            InternalTitle = internalTitle ?? string.Empty
        };
        Logger.Information("Created new article stub {Id}.", article.Id);
        return article;
    }
}
