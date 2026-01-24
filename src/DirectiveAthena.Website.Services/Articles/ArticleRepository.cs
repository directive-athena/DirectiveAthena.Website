// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(
    HttpClient http,
    IContentStorageFactory storageFactory,
    ILogger<ArticleRepository> logger
) : ContentRepository<Article>(
        http,
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
