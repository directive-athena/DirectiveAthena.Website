// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<ILocalFileStorage>]
public class LocalFileStorage(IJSRuntime jsRuntime, NavigationManager navigationManager, ILogger<LocalFileStorage> logger) : ILocalFileStorage {
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
    public async ValueTask<bool> IsSupportedAsync(CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.isSupported", ct);
        logger.Debug("Local file system support: {Supported}.", result);
        return result;
    }

    public async ValueTask<bool> RequestAccessAsync(CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.requestAccess", ct);
        logger.Information("Requested local file access: {Granted}.", result);
        return result;
    }

    public async ValueTask<bool> HasAccessAsync(CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.hasAccess", ct);
        logger.Debug("Local file access available: {Available}.", result);
        return result;
    }

    public async ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission", ct);
        logger.Debug("Local file permission verified: {Verified}.", result);
        return result;
    }

    public async ValueTask ResetAccessAsync(CancellationToken ct = default) {
        await jsRuntime.InvokeVoidAsync("fsApi.resetAccess", ct);
        logger.Information("Local file access reset.");
    }

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.writeFile", ct, relativePath, content);
        logger.Information("Write file {Path}: {Result}.", relativePath, result ? "success" : "failure");
        return result;
    }

    public async ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default) {
        string? result = await jsRuntime.InvokeAsync<string?>("fsApi.readFile", ct, relativePath);
        logger.Debug("Read file {Path}: {Result}.", relativePath, result is null ? "missing" : "ok");
        return result;
    }

    public async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default) {
        bool result = await jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", ct, relativePath);
        logger.Information("Delete file {Path}: {Result}.", relativePath, result ? "success" : "failure");
        return result;
    }
}
