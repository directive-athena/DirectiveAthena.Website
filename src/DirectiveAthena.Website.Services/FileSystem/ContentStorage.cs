// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ContentStorage(
    ILocalFileStorage fileStorage,
    ILocalizationProvider localizationProvider,
    string categoryFolder,
    ILogger logger
) : IContentStorage {
    private const string SourceRoot = "src/DirectiveAthena.Website/wwwroot";

    public bool IsLocalhost => fileStorage.IsLocalhost;
    public string IndexContentPath => $"{categoryFolder}/index.json";

    // -----------------------------------------------------------------------------------------------------------------
    // File Access
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> HasAccessAsync(CancellationToken ct = default)
        => fileStorage.HasAccessAsync(ct);

    public ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default)
        => fileStorage.VerifyPermissionAsync(ct);

    public ValueTask<bool> WriteFileAsync(string relativePath, string content, CancellationToken ct = default)
        => fileStorage.WriteFileAsync(relativePath, content, ct);

    public ValueTask<string?> ReadFileAsync(string relativePath, CancellationToken ct = default)
        => fileStorage.ReadFileAsync(relativePath, ct);

    public ValueTask<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default)
        => fileStorage.DeleteFileAsync(relativePath, ct);

    // -----------------------------------------------------------------------------------------------------------------
    // Path Helpers
    // -----------------------------------------------------------------------------------------------------------------
    public string GetMarkdownContentPath(string locale, string fileName)
        => $"{categoryFolder}/{locale}/{fileName}";

    public string GetIndexDiskPath()
        => $"{SourceRoot}/{categoryFolder}/index.json";

    public string GetMarkdownDiskPath(string locale, string fileName)
        => $"{SourceRoot}/{categoryFolder}/{locale}/{fileName}";

    // -----------------------------------------------------------------------------------------------------------------
    // Content Ops
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default)
        => fileStorage.WriteFileAsync(GetIndexDiskPath(), content, ct);

    public async Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default) {
        logger.Information("Deleting localized files for {FileName}.", fileName);
        Task<bool>[] deleteTasks = localizationProvider.GetSupportedLocalizations()
            .Select(culture => fileStorage.DeleteFileAsync(GetMarkdownDiskPath(culture.Code, fileName), ct).AsTask())
            .ToArray();

        bool[] results = await Task.WhenAll(deleteTasks);
        bool success = results.All(result => result);
        logger.Information("Delete localized files for {FileName} {Result}.", fileName, success ? "succeeded" : "failed");
        return success;
    }

}
