// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(
    IContentStorageFactory storageFactory,
    ILogger<ArticleRepository> logger
) : ContentRepository<Article>(
        storageFactory.ForCategory(ContentCategory.Articles),
        logger
    ),
    IArticleRepository {
    protected override string IndexPath => Storage.IndexContentPath;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetAllWithoutHiddenAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        Article[] results = ItemsById.Values.Where(p => !p.Hidden).ToArray();
        Logger.Debug("Filtered {Count} visible articles from {Total} cached items.", results.Length, ItemsById.Count);
        return results;
    }
}
