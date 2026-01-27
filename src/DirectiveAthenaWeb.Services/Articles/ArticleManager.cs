// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleManager>]
public class ArticleManager(
    ILocalizationProvider localizationProvider,
    IContentStorageFactory storageFactory,
    HttpClient http,
    IValidator<IEnumerable<Article>> validator,
    ILogger<ArticleManager> logger
) : IArticleManager {
    private readonly IContentStorage _storage = storageFactory.ForCategory(ContentCategory.Articles);

    public string GetLocalizedTitle(Article article)
        => GetLocalizedValue(article.Title);

    public string GetLocalizedSummary(Article article)
        => GetLocalizedValue(article.Summary);

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }

    public string GetLocalizedFilePath(Article article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return _storage.GetMarkdownContentPath(localization.Code, article.MarkdownFileName);
    }

    public async Task<string> GetRawMarkdownContentAsync(Article article, string locale, CancellationToken ct = default) {
        try {
            string path = _storage.GetMarkdownContentPath(locale, article.MarkdownFileName);
            logger.Debug("Fetching markdown for article {Id} at {Path}.", article.Id, path);
            return await http.GetStringAsync(path, ct);
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for article {Id} ({Locale}).", article.Id, locale);
            return string.Empty;
        }
    }

    public Article NewArticle() {
        var id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new Article {
            Id = id,
            Title = titles,
            Summary = summaries,
            Tags = [],
            CreatedAt = now,
            LastModifiedAt = now
        };
        logger.Information("Created new article stub {Id}.", article.Id);
        return article;
    }

    public bool Validate(IEnumerable<Article> articles, out string? errorMessage) {
        ValidationResult? result = validator.Validate(articles);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        logger.Warning("Article validation failed: {Error}.", errorMessage);
        return false;
    }

    public async Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(Article article, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {article.Title.GetValueOrDefault(c.Code)}");

        if (!writeToDisk) {
            logger.Debug("Generated article stubs for {Id} without writing to disk.", article.Id);
            return (stubs, false);
        }

        bool wroteAll = true;
        foreach (KeyValuePair<string, string> stub in stubs) {
            string path = _storage.GetMarkdownDiskPath(stub.Key, article.MarkdownFileName);
            if (!await _storage.WriteFileAsync(path, stub.Value, ct)) {
                wroteAll = false;
            }
        }

        logger.Information("Generated and wrote article stubs for {Id} {Result}.", article.Id, wroteAll ? "succeeded" : "failed");
        return (stubs, wroteAll);
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        _ = ct;
        logger.Debug("Resx generation is disabled in R2-only mode.");
        await Task.CompletedTask;
    }
}
