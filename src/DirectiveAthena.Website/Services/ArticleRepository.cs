// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json;
using CodeOfChaos.Extensions.DependencyInjection;
using System.Net.Http.Json;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(HttpClient http, IDevFileSystemManager devFs, ILocalizationProvider localizationProvider) : IArticleRepository {
    private Article[]? _articles;
    
    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<IEnumerable<Article>> GetPostsAsync(bool includeHidden = false) {
        if (_articles is not null) return includeHidden ? _articles : _articles.Where(p => !p.Hidden);

        try {
            _articles = await http.GetFromJsonAsync<Article[]>("content/articles/index.json");
            _articles ??= []; // if it is still null, set to an empty array
        }
        catch {
            _articles = [];
        }

        return includeHidden ? _articles : _articles.Where(p => !p.Hidden);
    }

    public async ValueTask<Article?> GetPostByIdAsync(string id) {
        IEnumerable<Article> articles = await GetPostsAsync(includeHidden: true);
        return articles.FirstOrDefault(p => p.Id == id);
    }

    public string AsJsonString(IEnumerable<Article> articles) 
        => JsonSerializer.Serialize(articles, Options);

    public async Task<bool> SaveAsync(IEnumerable<Article> articles) {
        if (!devFs.IsLocalhost) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        string json = AsJsonString(articles);
        return await devFs.WriteFileAsync(DevFileSystemPaths.GetIndexPath(), json);
    }
    
    public async Task<bool> DeleteAsync(Article article, IEnumerable<Article> articles) {
        if (!devFs.IsLocalhost || !await devFs.HasAccessAsync()) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        bool allDeleted = true;
        foreach (string path in localizationProvider.GetSupportedLocalizations()
            .Select(culture => DevFileSystemPaths.GetMarkdownPath(culture.Code, article.File))) {
            bool success = await devFs.DeleteFileAsync(path);
            if (!success) allDeleted = false;
        }

        if (!allDeleted) return false;

        return await SaveAsync(articles);
    }
}