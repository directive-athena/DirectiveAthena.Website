// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<INoteContentManager>]
[InjectableScoped<IContentManager<NoteContent>>]
internal class NoteContentManager(
    ILocalizationProvider localizationProvider,
    IR2Storage<NoteContent> storage,
    ILogger<ContentManagerBase<NoteContent>> logger
) : ContentManagerBase<NoteContent>(localizationProvider, storage, logger), INoteContentManager {
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public string GetLocalizedFilePath(NoteContent article) {
        LocalizationInfo localization = LocalizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }
    
    public override NoteContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = LocalizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new NoteContent {
            Id = id,
            LocalizedTitles = LocalizedDataHolder.FromDictionary(titles),
            LocalizedSummaries = LocalizedDataHolder.FromDictionary(summaries),
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
