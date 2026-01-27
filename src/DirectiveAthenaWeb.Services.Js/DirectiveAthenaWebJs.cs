// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace DirectiveAthenaWeb.Services.Js;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed class DirectiveAthenaWebJs(IJSRuntime jsRuntime, ILogger<DirectiveAthenaWebJs> logger) : IDirectiveAthenaWebJs {
    public async ValueTask<bool> CopyToClipboardAsync(string text, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("copyToClipboard", cancellationToken, text);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to copy text to clipboard.");
            return false;
        }
    }

    public async ValueTask DownloadFileAsync(string fileName, string content, CancellationToken cancellationToken = default) {
        try {
            await jsRuntime.InvokeVoidAsync("downloadFile", cancellationToken, fileName, content);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            logger.Error(e, "Failed to download file.");
        }
    }

    public async ValueTask<bool> IsFileSystemSupportedAsync(CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.isSupported", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to check if file system is supported.");
            return false;
        }
    }

    public async ValueTask<bool> RequestFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.requestAccess", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to request file system access.");
            return false;
        }
    }

    public async ValueTask<bool> HasFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.hasAccess", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to check if file system has access.");
            return false;
        }
    }

    public async ValueTask<bool> VerifyFileSystemPermissionAsync(CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to verify file system permission.");
            return false;
        }
    }

    public async ValueTask<bool> ResetFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.resetAccess", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to reset file system access.");
            return false;
        }
    }

    public async ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.writeFile", cancellationToken, relativePath, content);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to write file.");
            return false;
        }
    }

    public async ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<string?>("fsApi.readFile", cancellationToken, relativePath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return null;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to read file.");
            return null;
        }
    }

    public async ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", cancellationToken, relativePath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to delete file.");
            return false;
        }
    }

    public async ValueTask RegisterScrollListenerAsync<T>(DotNetObjectReference<T> dotNetHelper, CancellationToken cancellationToken = default)
        where T : class {
        try {
            await jsRuntime.InvokeVoidAsync("registerScrollListener", cancellationToken, dotNetHelper);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            logger.Error(e, "Failed to register scroll listener.");
        }
    }

    public async ValueTask ScrollToElementAsync(string elementId, CancellationToken cancellationToken = default) {
        try {
            await jsRuntime.InvokeVoidAsync("scrollToElement", cancellationToken, elementId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            logger.Error(e, "Failed to scroll to element.");
        }
    }

    public async ValueTask SetLocalStorageItemAsync(string key, string value, CancellationToken cancellationToken = default) {
        try {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, value);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
        catch (Exception e) {
            logger.Error(e, "Failed to set localStorage item.");
        }
    }

    public async ValueTask<string?> GetLocalStorageItemAsync(string key, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return null;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to get localStorage item.");
            return null;
        }
    }

    public async ValueTask<bool> ConfirmAsync(string message, CancellationToken cancellationToken = default) {
        try {
            return await jsRuntime.InvokeAsync<bool>("confirm", cancellationToken, message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch (Exception e) {
            logger.Error(e, "Failed to confirm.");
            return false;
        }
    }
}
