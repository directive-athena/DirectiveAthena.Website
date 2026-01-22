// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using System.Net.Http.Json;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IArticleRepository>]
public class ArticleRepository(HttpClient http) : IArticleRepository{
    private Article[]? _articles;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<IEnumerable<Article>> GetPostsAsync(bool includeHidden = false) {
        if (_articles is null) {
            try {
                _articles = await http.GetFromJsonAsync<Article[]>("content/articles/index.json");
                _articles ??= []; // if it is still null, set to an empty array
            }
            catch {
                _articles = [];
            }
        }

        return includeHidden ? _articles : _articles.Where(p => !p.Hidden).ToArray();
    }

    public async ValueTask<Article?> GetPostByIdAsync(string id) {
        IEnumerable<Article> articles = await GetPostsAsync(includeHidden: true);
        return articles.FirstOrDefault(p => p.Id == id);
    }
}