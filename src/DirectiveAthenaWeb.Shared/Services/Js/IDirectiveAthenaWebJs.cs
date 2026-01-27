// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.JSInterop;

namespace DirectiveAthenaWeb.Services.Js;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------\
public interface IDirectiveAthenaWebJs {
    ValueTask<bool> CopyToClipboardAsync(string text, CancellationToken cancellationToken = default);
    ValueTask DownloadFileAsync(string fileName, string content, CancellationToken cancellationToken = default);

    ValueTask<bool> IsFileSystemSupportedAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> RequestFileSystemAccessAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> HasFileSystemAccessAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> VerifyFileSystemPermissionAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> ResetFileSystemAccessAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken cancellationToken = default);
    ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken cancellationToken = default);
    ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);

    ValueTask RegisterScrollListenerAsync<T>(DotNetObjectReference<T> dotNetHelper, CancellationToken cancellationToken = default) where T : class;
    ValueTask ScrollToElementAsync(string elementId, CancellationToken cancellationToken = default);

    ValueTask SetLocalStorageItemAsync(string key, string value, CancellationToken cancellationToken = default);
    ValueTask<string?> GetLocalStorageItemAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<bool> ConfirmAsync(string message, CancellationToken cancellationToken = default);
}
