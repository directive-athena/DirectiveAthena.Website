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
    public ValueTask<bool> CopyToClipboardAsync(string text, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("copyToClipboard", cancellationToken, text);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to copy text to clipboard.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask DownloadFileAsync(string fileName, string content, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeVoidAsync("downloadFile", cancellationToken, fileName, content);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to download file.");
            return ValueTask.CompletedTask;
        }
    }

    public ValueTask<bool> IsFileSystemSupportedAsync(CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.isSupported", cancellationToken);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to check if file system is supported.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> RequestFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.requestAccess", cancellationToken);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to request file system access.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> HasFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.hasAccess", cancellationToken);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to check if file system has access.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> VerifyFileSystemPermissionAsync(CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission", cancellationToken);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to verify file system permission.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> ResetFileSystemAccessAsync(CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.resetAccess", cancellationToken);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to reset file system access.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.writeFile", cancellationToken, relativePath, content);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to write file.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<string?>("fsApi.readFile", cancellationToken, relativePath);

        }
        catch (Exception e) {
            logger.Error(e, "Failed to read file.");
            return ValueTask.FromResult<string?>(null);
        }
    }

    public ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", cancellationToken, relativePath);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to delete file.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask RegisterScrollListenerAsync<T>(DotNetObjectReference<T> dotNetHelper, CancellationToken cancellationToken = default)
        where T : class {
        try {
            return jsRuntime.InvokeVoidAsync("registerScrollListener", cancellationToken, dotNetHelper);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to register scroll listener.");
            return ValueTask.CompletedTask;
        }
    }

    public ValueTask ScrollToElementAsync(string elementId, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeVoidAsync("scrollToElement", cancellationToken, elementId);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to scroll to element.");
            return ValueTask.CompletedTask;
        }
    }

    public ValueTask SetLocalStorageItemAsync(string key, string value, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, value);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to set localStorage item.");
            return ValueTask.CompletedTask;
        }
    }

    public ValueTask<string?> GetLocalStorageItemAsync(string key, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to get localStorage item.");
            return ValueTask.FromResult<string?>(null);
        }
    }

    public ValueTask<bool> ConfirmAsync(string message, CancellationToken cancellationToken = default) {
        try {
            return jsRuntime.InvokeAsync<bool>("confirm", cancellationToken, message);
        }
        catch (Exception e) {
            logger.Error(e, "Failed to confirm.");
            return ValueTask.FromResult(false);
        }
    }
}
