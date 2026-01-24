// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(
    HttpClient http,
    IContentStorageFactory storageFactory
) : ContentRepository<Article>(
    http,
    storageFactory.ForCategory(ContentCategory.Articles)
), IArticleRepository {
    protected override string IndexPath => Storage.IndexContentPath;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetAllWithoutHiddenAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        return ItemsById.Values.Where(p => !p.Hidden).ToArray();
    }
}
