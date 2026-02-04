// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Content;
using System.Net.Http.Headers;

namespace DirectiveAthenaWeb.Services.R2Storage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
// ReSharper disable once UnusedTypeParameter
public interface IR2Storage<TContent> where TContent : IContent {
    string IndexContentPath { get; }
    string GetMarkdownContentPath(string locale, string fileName);
    string GetMarkdownDiskPath(string locale, string fileName);
    ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default);
    Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default);
    ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default);
    ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default);
    Task<R2ReadResult> ReadIndexAsync(EntityTagHeaderValue? etag, DateTimeOffset? lastModifiedUtc, CancellationToken ct = default);
}