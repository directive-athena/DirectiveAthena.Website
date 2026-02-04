// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using FluentValidation;
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
    IValidator<NoteContent> singleValidator,
    IValidator<IEnumerable<NoteContent>> multipleValidator,
    ILogger<ContentManagerBase<NoteContent>> logger
) : ContentManagerBase<NoteContent>(storage, singleValidator, multipleValidator, logger), INoteContentManager {
    private readonly IR2Storage<NoteContent> _storage = storage;
    private readonly ILogger<ContentManagerBase<NoteContent>> _logger = logger;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
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
        return _storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
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
        _logger.Information("Created new article stub {Id}.", article.Id);
        return article;
    }
}
