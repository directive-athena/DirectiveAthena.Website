// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.FileSystem;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IResourceStorage {
    bool IsLocalhost { get; }
    ValueTask<bool> HasAccessAsync(CancellationToken ct = default);
    ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default);
    string GetSharedResxPath(string locale);
    string GetTagsResxPath(string locale);
    ValueTask<string?> ReadSharedResxAsync(string locale, CancellationToken ct = default);
    ValueTask<bool> WriteSharedResxAsync(string locale, string content, CancellationToken ct = default);
    ValueTask<string?> ReadTagsResxAsync(string locale, CancellationToken ct = default);
    ValueTask<bool> WriteTagsResxAsync(string locale, string content, CancellationToken ct = default);
}
