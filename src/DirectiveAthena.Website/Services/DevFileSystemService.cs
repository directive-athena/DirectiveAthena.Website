// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class DevFileSystemService(IJSRuntime jsRuntime, NavigationManager navigationManager) {
    public bool IsLocalhost {
        get {
#if DEBUG
            return navigationManager.BaseUri.Contains("localhost") || navigationManager.BaseUri.Contains("127.0.0.1");
#else
            return false;
#endif
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<bool> IsSupportedAsync() 
        => await jsRuntime.InvokeAsync<bool>("fsApi.isSupported");

    public async ValueTask<bool> RequestAccessAsync() 
        => await jsRuntime.InvokeAsync<bool>("fsApi.requestAccess");

    public async ValueTask<bool> HasAccessAsync() 
        => await jsRuntime.InvokeAsync<bool>("fsApi.hasAccess");

    public async ValueTask<bool> VerifyPermissionAsync() 
        => await jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission");

    public async ValueTask ResetAccessAsync() 
        => await jsRuntime.InvokeVoidAsync("fsApi.resetAccess");

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content) 
        => await jsRuntime.InvokeAsync<bool>("fsApi.writeFile", relativePath, content);

    public async ValueTask<string?> ReadFileAsync(string relativePath) 
        => await jsRuntime.InvokeAsync<string?>("fsApi.readFile", relativePath);

    public static string GetIndexPath() 
        => "src/DirectiveAthena.Website/wwwroot/content/writings/index.json";

    public static string GetMarkdownPath(string locale, string fileName) 
        => $"src/DirectiveAthena.Website/wwwroot/content/writings/{locale}/{fileName}";

    public static string GetSharedResxPath(string locale) 
        => locale == LocalizationConfig.DefaultCulture.Code
            ? "src/DirectiveAthena.Website/Resources/Shared.resx"
            : $"src/DirectiveAthena.Website/Resources/Shared.{locale}.resx";

    public static string GetTagsResxPath(string locale)
        => locale == LocalizationConfig.DefaultCulture.Code
            ? "src/DirectiveAthena.Website/Resources/Tags.resx"
            : $"src/DirectiveAthena.Website/Resources/Tags.{locale}.resx";
}