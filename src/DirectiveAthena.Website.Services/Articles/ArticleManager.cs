// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Linq;
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleManager>]
public class ArticleManager(
    ILocalizationProvider localizationProvider,
    IContentStorageFactory storageFactory,
    IResourceStorage resourceStorage,
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
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        var article = new Article {
            Id = id,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = titles,
            Summary = summaries,
            Tags = [],
            Hidden = false
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

    public async Task<Dictionary<string, string>> GenerateStubsAsync(Article article, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {article.Title.GetValueOrDefault(c.Code)}");

        if (!writeToDisk || !_storage.IsWritable || !await _storage.HasAccessAsync() || !await _storage.VerifyPermissionAsync()) {
            logger.Debug("Generated article stubs for {Id} without writing to disk.", article.Id);
            return stubs;
        }

        foreach (KeyValuePair<string, string> stub in stubs) {
            string path = _storage.GetMarkdownDiskPath(stub.Key, article.MarkdownFileName);
            await _storage.WriteFileAsync(path, stub.Value);
        }

        logger.Information("Generated and wrote article stubs for {Id}.", article.Id);
        return stubs;
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        if (!resourceStorage.IsLocalhost || !await resourceStorage.HasAccessAsync()) {
            logger.Debug("Skipping resx generation because resource storage is unavailable.");
            return;
        }

        foreach (LocalizationInfo culture in localizationProvider.GetSupportedLocalizations()) {
            string? content = await resourceStorage.ReadSharedResxAsync(culture.Code, ct);
            if (content is not null) {
                logger.Debug("Shared resx already exists for {Culture}.", culture.Code);
                continue;
            }

            XDocument newResx = CreateNewResx();
            await resourceStorage.WriteSharedResxAsync(culture.Code, newResx.ToString(), ct);
            logger.Information("Created shared resx for {Culture}.", culture.Code);
        }
    }

    private static XDocument CreateNewResx() {
        return new XDocument(
            new XElement("root",
                new XElement("resheader", new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")),
                new XElement("resheader", new XAttribute("name", "version"), new XElement("value", "2.0")),
                new XElement("resheader", new XAttribute("name", "reader"),
                    new XElement("value",
                        "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                new XElement("resheader", new XAttribute("name", "writer"),
                    new XElement("value",
                        "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
            )
        );
    }
}
