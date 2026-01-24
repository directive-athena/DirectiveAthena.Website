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
    IDevFileSystemManager devFs,
    HttpClient http,
    IValidator<IEnumerable<Article>> validator,
    IDevFileSystemPaths devFsPaths
) : IArticleManager {
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
        return $"content/articles/{localization.Code}/{article.MarkdownFileName}";
    }

    public async Task<string> GetRawMarkdownContentAsync(Article article, string locale, CancellationToken ct = default) {
        try {
            return await http.GetStringAsync($"content/articles/{locale}/{article.MarkdownFileName}", ct);
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

        if (!writeToDisk || !devFs.IsLocalhost || !await devFs.HasAccessAsync() || !await devFs.VerifyPermissionAsync()) return stubs;

        foreach (KeyValuePair<string, string> stub in stubs) {
            await devFs.WriteFileAsync(devFsPaths.GetMarkdownPath(stub.Key, article.MarkdownFileName), stub.Value);
        }

        return stubs;
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        if (!devFs.IsLocalhost || !await devFs.HasAccessAsync()) return;

        foreach (string path in localizationProvider.GetSupportedLocalizations()
            .Select(culture => devFsPaths.GetSharedResxPath(culture.Code))) {
            string? content = await devFs.ReadFileAsync(path);
            if (content is not null) continue;

            XDocument newResx = CreateNewResx();
            await devFs.WriteFileAsync(path, newResx.ToString());
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
