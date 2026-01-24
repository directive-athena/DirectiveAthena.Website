// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json;
using CodeOfChaos.Extensions.DependencyInjection;
using System.Net.Http.Json;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using System.Collections.Immutable;

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
) : IArticleRepository {
    private readonly SemaphoreSlim _lock = new(1, 1);
    private ImmutableArray<Article> _articles;
    
    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<Article[]> GetPostsAsync(bool includeHidden = false, CancellationToken ct = default) {
        if (!_articles.IsDefaultOrEmpty) return includeHidden ? _articles.ToArray() : _articles.Where(p => !p.Hidden).ToArray();

        await _lock.WaitAsync(ct);
        try {
            if (!_articles.IsDefaultOrEmpty) return includeHidden ? _articles.ToArray() : _articles.Where(p => !p.Hidden).ToArray();

            Article[]? articles =  await http.GetFromJsonAsync<Article[]>("content/articles/index.json", ct);
            _articles = [..articles ?? []];
        }
        catch {
            _articles = [];
        }
        finally {
            _lock.Release();
        }

        return includeHidden ? _articles.ToArray() : _articles.Where(p => !p.Hidden).ToArray();
    }

    public async ValueTask<Article?> GetPostByIdAsync(string id, CancellationToken ct = default) {
        IEnumerable<Article> articles = await GetPostsAsync(includeHidden: true, ct: ct);
        return articles.FirstOrDefault(p => p.Id == id);
    }

    public string AsJsonString(IEnumerable<Article> articles) 
        => JsonSerializer.Serialize(articles, Options);

    public async Task<bool> SaveAsync(IEnumerable<Article> articles, CancellationToken ct = default) {
        if (!devFs.IsLocalhost) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        string json = AsJsonString(articles);
        return await devFs.WriteFileAsync(devFsPaths.GetIndexPath(), json);
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
