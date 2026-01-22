// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Xml.Linq;
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleManager>]
public class ArticleManager(
    ILocalizationProvider localizationProvider,
    IDevFileSystemManager devFs,
    HttpClient http
) : IArticleManager {
    public string GetLocalizedTitle(Article article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !article.Title.TryGetValue(localization.Code, out string? title)
            ? article.Title.GetValueOrDefault(LocalizationProvider.DefaultLocalization.Code, string.Empty)
            : title;
    }

    public string GetLocalizedSummary(Article article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !article.Summary.TryGetValue(localization.Code, out string? summary)
            ? article.Summary.GetValueOrDefault(LocalizationProvider.DefaultLocalization.Code, string.Empty)
            : summary;
    }

    public string GetLocalizedFilePath(Article article) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return $"content/articles/{localization.Code}/{article.File}";
    }

    public async Task<string> GetRawMarkdownContentAsync(Article article, string locale) {
        try {
            return await http.GetStringAsync($"content/articles/{locale}/{article.File}");
        }
        catch {
            return string.Empty;
        }
    }

    public Article NewArticle() {
        string id = Guid.NewGuid().ToString();
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        
        Dictionary<string, string> titles = locals.ToDictionary(c => c.Code, _ => "New Post");
        Dictionary<string, string> summaries = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Summary here");
        
        return new Article {
            Id = id,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = titles,
            Summary = summaries,
            File = $"{id}.md",
            Tags = [],
            Hidden = false
        };
    }

    public bool Validate(IEnumerable<Article> articles, out string? errorMessage) {
        if (articles.Any(p => string.IsNullOrWhiteSpace(p.Id) || string.IsNullOrWhiteSpace(p.File))) {
            errorMessage = "Some posts have missing Id or File!";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public async Task<Dictionary<string, string>> GenerateStubsAsync(Article article, bool writeToDisk = false) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {article.Title.GetValueOrDefault(c.Code)}");

        if (!writeToDisk || !devFs.IsLocalhost || !await devFs.HasAccessAsync() || !await devFs.VerifyPermissionAsync()) return stubs;

        foreach (KeyValuePair<string, string> stub in stubs) {
            await devFs.WriteFileAsync(DevFileSystemPaths.GetMarkdownPath(stub.Key, article.File), stub.Value);
        }

        return stubs;
    }

    public async Task EnsureResxAsync() {
        if (!devFs.IsLocalhost || !await devFs.HasAccessAsync()) return;

        foreach (string path in localizationProvider.GetSupportedLocalizations()
            .Select(culture => DevFileSystemPaths.GetSharedResxPath(culture.Code))) {
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
