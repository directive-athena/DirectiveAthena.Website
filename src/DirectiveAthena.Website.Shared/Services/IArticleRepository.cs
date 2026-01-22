// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IArticleRepository {
    ValueTask<IEnumerable<Article>> GetPostsAsync(bool includeHidden = false);
    ValueTask<Article?> GetPostByIdAsync(string id);
}
