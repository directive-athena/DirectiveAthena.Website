// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IDevFileSystemManager {
    bool IsLocalhost { get; }
    
    ValueTask<bool> IsSupportedAsync();
    ValueTask<bool> RequestAccessAsync();
    ValueTask<bool> HasAccessAsync();
    ValueTask<bool> VerifyPermissionAsync();
    ValueTask ResetAccessAsync();
    ValueTask<bool> WriteFileAsync(string relativePath, string content);
    ValueTask<string?> ReadFileAsync(string relativePath);
    ValueTask<bool> DeleteFileAsync(string relativePath);
}
