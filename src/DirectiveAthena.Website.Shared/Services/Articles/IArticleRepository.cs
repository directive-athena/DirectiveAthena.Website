// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IArticleRepository {
    ValueTask<Article[]> GetPostsAsync(bool includeHidden = false, CancellationToken ct = default);
    ValueTask<Article?> GetPostByIdAsync(string id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Article article, IEnumerable<Article> articles, CancellationToken ct = default);
    
    string AsJsonString(IEnumerable<Article> articles);
    Task<bool> SaveAsync(IEnumerable<Article> articles, CancellationToken ct = default);
}
