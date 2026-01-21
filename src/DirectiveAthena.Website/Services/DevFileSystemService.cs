using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using DirectiveAthena.Website.Models;

namespace DirectiveAthena.Website.Services;

public class DevFileSystemService(IJSRuntime js, NavigationManager nav) {
    public bool IsLocalhost {
        get {
            #if DEBUG
            return nav.BaseUri.Contains("localhost") || nav.BaseUri.Contains("127.0.0.1");
            #else
            return false;
            #endif
        }
    }

    public async ValueTask<bool> IsSupportedAsync() {
        return await js.InvokeAsync<bool>("fsApi.isSupported");
    }

    public async ValueTask<bool> RequestAccessAsync() {
        return await js.InvokeAsync<bool>("fsApi.requestAccess");
    }

    public async ValueTask<bool> HasAccessAsync() {
        return await js.InvokeAsync<bool>("fsApi.hasAccess");
    }

    public async ValueTask<bool> VerifyPermissionAsync() {
        return await js.InvokeAsync<bool>("fsApi.verifyPermission");
    }

    public async ValueTask ResetAccessAsync() {
        await js.InvokeVoidAsync("fsApi.resetAccess");
    }

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content) {
        return await js.InvokeAsync<bool>("fsApi.writeFile", relativePath, content);
    }

    public async ValueTask<string?> ReadFileAsync(string relativePath) {
        return await js.InvokeAsync<string?>("fsApi.readFile", relativePath);
    }

    public static string GetIndexPath() => "src/DirectiveAthena.Website/wwwroot/content/writings/index.json";

    public static string GetMarkdownPath(string locale, string fileName) =>
        $"src/DirectiveAthena.Website/wwwroot/content/writings/{locale}/{fileName}";

    public static string GetResxPath(string locale) => locale == LocalizationConfig.DefaultCulture.Code
        ? "src/DirectiveAthena.Website/Resources/Shared.resx"
        : $"src/DirectiveAthena.Website/Resources/Shared.{locale}.resx";
}