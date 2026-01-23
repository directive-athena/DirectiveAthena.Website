// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace DirectiveAthena.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IDevFileSystemManager>]
public class DevFileSystemManager(IJSRuntime jsRuntime, NavigationManager navigationManager) : IDevFileSystemManager {
    public bool IsLocalhost {
        get {
            #if DEBUG
            return navigationManager.BaseUri.Contains("localhost") || navigationManager.BaseUri.Contains("127.0.0.1");
            #else
            _ = navigationManager; // keep parameter considered used in Release
            return false;
            #endif
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> IsSupportedAsync()
        => jsRuntime.InvokeAsync<bool>("fsApi.isSupported");

    public ValueTask<bool> RequestAccessAsync()
        => jsRuntime.InvokeAsync<bool>("fsApi.requestAccess");

    public ValueTask<bool> HasAccessAsync()
        => jsRuntime.InvokeAsync<bool>("fsApi.hasAccess");

    public ValueTask<bool> VerifyPermissionAsync()
        => jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission");

    public ValueTask ResetAccessAsync()
        => jsRuntime.InvokeVoidAsync("fsApi.resetAccess");

    public ValueTask<bool> WriteFileAsync(string relativePath, string content)
        => jsRuntime.InvokeAsync<bool>("fsApi.writeFile", relativePath, content);

    public ValueTask<string?> ReadFileAsync(string relativePath)
        => jsRuntime.InvokeAsync<string?>("fsApi.readFile", relativePath);

    public ValueTask<bool> DeleteFileAsync(string relativePath)
        => jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", relativePath);
}