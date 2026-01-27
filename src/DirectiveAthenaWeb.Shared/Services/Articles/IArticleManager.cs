// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IArticleManager {
    string GetLocalizedTitle(Article article);
    string GetLocalizedSummary(Article article);
    string GetLocalizedFilePath(Article article);
    Task<string> GetRawMarkdownContentAsync(Article article, string locale, CancellationToken ct = default);

    Article NewArticle();
    bool Validate(IEnumerable<Article> articles, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(Article article, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
