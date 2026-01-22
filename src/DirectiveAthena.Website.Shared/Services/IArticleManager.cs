// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IArticleManager {
    string GetLocalizedTitle(Article article);
    string GetLocalizedSummary(Article article);
    string GetLocalizedFilePath(Article article);
    Task<string> GetRawMarkdownContentAsync(Article article, string locale);

    Article NewArticle();
    bool Validate(IEnumerable<Article> articles, out string? errorMessage);
    Task<Dictionary<string, string>> GenerateStubsAsync(Article article, bool writeToDisk = false);
    Task EnsureResxAsync();
}
