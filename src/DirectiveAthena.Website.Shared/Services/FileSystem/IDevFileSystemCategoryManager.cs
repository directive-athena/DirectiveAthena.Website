// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IDevFileSystemCategoryManager {
    bool IsLocalhost { get; }
    ValueTask<bool> HasAccessAsync(CancellationToken ct = default);
    ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default);
    ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default);
    Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default);
}
