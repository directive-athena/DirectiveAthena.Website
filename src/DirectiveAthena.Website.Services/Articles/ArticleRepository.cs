// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json;
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
) : CachedJsonRepository<Article>(http, devFs), IArticleRepository {

    protected override string IndexPath => "content/articles/index.json";
    protected override string WritePath => devFsPaths.GetIndexPath();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetPostsAsync(bool includeHidden = false, CancellationToken ct = default) {
        var articles = await GetAllAsync(ct);
        return includeHidden ? articles.ToArray() : articles.Where(p => !p.Hidden).ToArray();
    }

    public async ValueTask<Article?> GetPostByIdAsync(string id, CancellationToken ct = default) {
        var articles = await GetAllAsync(ct);
        return articles.FirstOrDefault(p => p.Id == id);
    }
    
    public async Task<bool> DeleteAsync(Article article, IEnumerable<Article> articles, CancellationToken ct = default) {
        if (!devFs.IsLocalhost || !await devFs.HasAccessAsync()) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        bool allDeleted = true;
        foreach (string path in localizationProvider.GetSupportedLocalizations()
            .Select(culture => devFsPaths.GetMarkdownPath(culture.Code, article.File))) {
            bool success = await devFs.DeleteFileAsync(path);
            if (!success) allDeleted = false;
        }

        if (!allDeleted) return false;

        return await SaveAsync(articles, ct);
    }
}
