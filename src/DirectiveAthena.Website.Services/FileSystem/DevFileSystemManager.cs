// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace DirectiveAthena.Website.Services.FileSystem;
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
    public ValueTask<bool> IsSupportedAsync(CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.isSupported", ct);

    public ValueTask<bool> RequestAccessAsync(CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.requestAccess", ct);

    public ValueTask<bool> HasAccessAsync(CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.hasAccess", ct);

    public ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission", ct);

    public ValueTask ResetAccessAsync(CancellationToken ct = default)
        => jsRuntime.InvokeVoidAsync("fsApi.resetAccess", ct);

    public ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.writeFile", ct, relativePath, content);

    public ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default)
        => jsRuntime.InvokeAsync<string?>("fsApi.readFile", ct, relativePath);

    public ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default)
        => jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", ct, relativePath);
}
