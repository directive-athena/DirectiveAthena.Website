// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IDevFileSystemManager {
    bool IsLocalhost { get; }
    
    ValueTask<bool> IsSupportedAsync(CancellationToken ct = default);
    ValueTask<bool> RequestAccessAsync(CancellationToken ct = default);
    ValueTask<bool> HasAccessAsync(CancellationToken ct = default);
    ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default);
    ValueTask ResetAccessAsync(CancellationToken ct = default);
    ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default);
    ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default);
    ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default);
}
