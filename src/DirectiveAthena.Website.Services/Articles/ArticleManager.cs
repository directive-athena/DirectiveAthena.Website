// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Linq;
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using FluentValidation;
using FluentValidation.Results;

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
    IValidator<IEnumerable<Article>> validator
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
            return await http.GetStringAsync(path, ct);
        }
        catch {
            return string.Empty;
        }
    }

    public Article NewArticle() {
        var id = Guid.CreateVersion7();
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        
        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        
        return new Article {
            Id = id,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = titles,
            Summary = summaries,
            Tags = [],
            Hidden = false
        };
    }

    public bool Validate(IEnumerable<Article> articles, out string? errorMessage) {
        ValidationResult? result = validator.Validate(articles);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        return false;
    }

    public async Task<Dictionary<string, string>> GenerateStubsAsync(Article article, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {article.Title.GetValueOrDefault(c.Code)}");

        if (!writeToDisk || !_storage.IsLocalhost || !await _storage.HasAccessAsync() || !await _storage.VerifyPermissionAsync()) return stubs;

        foreach (KeyValuePair<string, string> stub in stubs) {
            string path = _storage.GetMarkdownDiskPath(stub.Key, article.MarkdownFileName);
            await _storage.WriteFileAsync(path, stub.Value);
        }

        return stubs;
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        if (!resourceStorage.IsLocalhost || !await resourceStorage.HasAccessAsync()) return;

        foreach (LocalizationInfo culture in localizationProvider.GetSupportedLocalizations()) {
            string? content = await resourceStorage.ReadSharedResxAsync(culture.Code, ct);
            if (content is not null) continue;

            XDocument newResx = CreateNewResx();
            await resourceStorage.WriteSharedResxAsync(culture.Code, newResx.ToString(), ct);
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
