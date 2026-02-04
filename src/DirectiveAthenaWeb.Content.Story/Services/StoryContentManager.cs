// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IStoryContentManager>]
[InjectableScoped<IContentManager<StoryContent>>]
internal class StoryContentManager(
    ILocalizationProvider localizationProvider,
    IServiceProvider provider
) : ContentManagerBase<StoryContent>(provider), IStoryContentManager {

    public string GetLocalizedTitle(StoryContent article)
        => GetLocalizedValue(article.Title);

    public string GetLocalizedSummary(StoryContent article)
        => GetLocalizedValue(article.Summary);

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }

    public string GetLocalizedFilePath(StoryContent article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }

    public override StoryContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Story");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new StoryContent {
            Id = id,
            Title = titles,
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
