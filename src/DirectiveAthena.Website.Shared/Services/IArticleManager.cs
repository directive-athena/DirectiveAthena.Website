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
}
