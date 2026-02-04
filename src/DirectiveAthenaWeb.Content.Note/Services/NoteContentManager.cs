// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<INoteContentManager>]
[InjectableScoped<IContentManager<NoteContent>>]
internal class NoteContentManager(
    ILocalizationProvider localizationProvider,
    IServiceProvider provider
) : ContentManagerBase<NoteContent>(provider), INoteContentManager {

    public string GetLocalizedTitle(NoteContent article)
        => GetLocalizedValue(article.Title);

    public string GetLocalizedSummary(NoteContent article)
        => GetLocalizedValue(article.Summary);

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }

    public string GetLocalizedFilePath(NoteContent article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }
    
    public override NoteContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new NoteContent {
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
