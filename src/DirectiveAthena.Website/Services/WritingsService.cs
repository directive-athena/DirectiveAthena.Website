// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Globalization;
using System.Net.Http.Json;
using DirectiveAthena.Website.Models;
using DirectiveAthena.Website.Resources;
using Microsoft.Extensions.Localization;

namespace DirectiveAthena.Website.Services;

public class WritingsService(HttpClient http, IStringLocalizer<Shared> localizer) {
    private Post[]? _posts;

    public async ValueTask<Post[]> GetPostsAsync() {
        if (_posts is not null) return _posts;
        try {
            _posts = await http.GetFromJsonAsync<Post[]>("content/writings/index.json");
        }
        catch {
            _posts = [];
        }

        return _posts ?? [];
    }

    public async Task<Post?> GetPostBySlugAsync(string slug) {
        var posts = await GetPostsAsync();
        return posts.FirstOrDefault(p => p.Slug == slug);
    }

    public string GetLocalizedTitle(Post post) {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (post.Title.TryGetValue(culture, out string? title)) return title;
        return post.Title.GetValueOrDefault(LocalizationConfig.DefaultCulture.Code) ?? "";
    }

    public string GetLocalizedSummary(Post post) {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (post.Summary.TryGetValue(culture, out string? summary)) return summary;
        return post.Summary.GetValueOrDefault(LocalizationConfig.DefaultCulture.Code) ?? "";
    }

    public string GetLocalizedFilePath(Post post) {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return $"content/writings/{culture}/{post.File}";
    }

    public string GetLocalizedTagName(string tagId) {
        LocalizedString localized = localizer[$"Tag.{tagId}"];
        return localized.ResourceNotFound ? tagId : localized.Value;
    }

    public async Task<string> GetRawMarkdownAsync(string locale, string fileName) {
        try {
            return await http.GetStringAsync($"content/writings/{locale}/{fileName}");
        }
        catch {
            return string.Empty;
        }
    }
}