// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.ContentStorage;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentStorage {
    bool IsWritable { get; }
    ValueTask<bool> HasAccessAsync(CancellationToken ct = default);
    ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default);
    string IndexContentPath { get; }
    string GetMarkdownContentPath(string locale, string fileName);
    string GetIndexDiskPath();
    string GetMarkdownDiskPath(string locale, string fileName);
    ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default);
    Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default);
    ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default);
    ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default);
    ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default);
}
