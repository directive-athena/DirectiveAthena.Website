// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleManager>]
public class ArticleManager(ILocalizationProvider localizationProvider, HttpClient http) : IArticleManager {
    public string GetLocalizedTitle(Article article) {
        LocalizationInfo localization= localizationProvider.GetCurrentLocalization();
        return !article.Title.TryGetValue(localization.Code, out string? title) 
            ? article.Title.GetValueOrDefault(LocalizationProvider.DefaultLocalization.Code, string.Empty) 
            : title;
    }

    public string GetLocalizedSummary(Article article) {
        LocalizationInfo localization= localizationProvider.GetCurrentLocalization();
        return !article.Summary.TryGetValue(localization.Code, out string? summary) 
            ? article.Summary.GetValueOrDefault(LocalizationProvider.DefaultLocalization.Code, string.Empty) 
            : summary;
    }

    public string GetLocalizedFilePath(Article article) {
        LocalizationInfo localization= localizationProvider.GetCurrentLocalization();
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
}
