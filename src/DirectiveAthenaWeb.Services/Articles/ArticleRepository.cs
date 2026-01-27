// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(
    [FromKeyedServices(ContentCategory.Articles)] IContentStorage storage,
    ILogger<ArticleRepository> logger
) : ContentRepository<Article>(storage, logger), IArticleRepository {

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetAllWithoutHiddenAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        Article[] results = ItemsById.Values.Where(p => !p.IsHidden && !p.IsSoftDeleted).ToArray();
        Logger.Debug("Filtered {Count} visible articles from {Total} cached items.", results.Length, ItemsById.Count);
        return results;
    }
}
