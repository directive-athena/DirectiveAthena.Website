// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using FluentValidation;
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
    IValidator<StoryContent> singleValidator,
    IValidator<IEnumerable<StoryContent>> multipleValidator,
    ILogger<ContentManagerBase<StoryContent>> logger
) : ContentManagerBase<StoryContent>(storage, singleValidator, multipleValidator, logger), IStoryContentManager {
    private readonly IR2Storage<StoryContent> _storage = storage;
    private readonly ILogger<ContentManagerBase<StoryContent>> _logger = logger;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
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
        return _storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
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
        _logger.Information("Created new article stub {Id}.", article.Id);
        return article;
    }
}
