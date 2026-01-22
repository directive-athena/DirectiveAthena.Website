// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IArticleRepository {
    ValueTask<IEnumerable<Article>> GetPostsAsync(bool includeHidden = false, CancellationToken ct = default);
    ValueTask<Article?> GetPostByIdAsync(string id, CancellationToken ct = default);
    Task<bool> DeleteAsync(Article article, IEnumerable<Article> articles, CancellationToken ct = default);
    
    string AsJsonString(IEnumerable<Article> articles);
    Task<bool> SaveAsync(IEnumerable<Article> articles, CancellationToken ct = default);
}
