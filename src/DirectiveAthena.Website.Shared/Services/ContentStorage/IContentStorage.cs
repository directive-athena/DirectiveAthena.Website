// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.ContentStorage;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentStorage {
    string IndexContentPath { get; }
    string GetMarkdownContentPath(string locale, string fileName);
    string GetMarkdownDiskPath(string locale, string fileName);
    ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default);
    Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default);
    ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default);
}
