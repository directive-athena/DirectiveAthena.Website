// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(
    HttpClient http,
    IDevFileSystemManager devFs,
    ILocalizationProvider localizationProvider,
    IDevFileSystemPaths devFsPaths
) : ContentRepository<Article>(
    http,
    new DevFileSystemCategoryManager(devFs, devFsPaths, localizationProvider, DevFileSystemCategory.Articles)
), IArticleRepository {
    protected override string IndexPath => "content/articles/index.json";

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetAllWithoutHiddenAsync(CancellationToken ct = default) {
        await EnsureCacheAsync(ct);
        return ItemsById.Values.Where(p => !p.Hidden).ToArray();
    }
}
