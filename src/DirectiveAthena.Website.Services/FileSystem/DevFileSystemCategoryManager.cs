// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed class DevFileSystemCategoryManager(
    IDevFileSystemManager devFs,
    IDevFileSystemPaths devFsPaths,
    ILocalizationProvider localizationProvider,
    DevFileSystemCategory category
) : IDevFileSystemCategoryManager {
    public bool IsLocalhost => devFs.IsLocalhost;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> HasAccessAsync(CancellationToken ct = default)
        => devFs.HasAccessAsync(ct);

    public ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default)    
        => devFs.VerifyPermissionAsync(ct);

    public ValueTask<bool> WriteIndexAsync(string content, CancellationToken ct = default)
        => devFs.WriteFileAsync(GetIndexPath(), content, ct);

    public async Task<bool> DeleteLocalizedFilesAsync(string fileName, CancellationToken ct = default) {
        Task<bool>[] deleteTasks = GetLocalizedPaths(fileName)
            .Select(path => devFs.DeleteFileAsync(path, ct).AsTask())
            .ToArray();

        bool[] results = await Task.WhenAll(deleteTasks);
        return results.All(success => success);
    }

    private string GetIndexPath()
        => category switch {
            DevFileSystemCategory.Articles => devFsPaths.GetIndexPath(),
            DevFileSystemCategory.WorldRules => devFsPaths.GetWorldRulesIndexPath(),
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported DevFS category.")
        };

    private IEnumerable<string> GetLocalizedPaths(string fileName)
        => category switch {
            DevFileSystemCategory.Articles => localizationProvider.GetSupportedLocalizations()
                .Select(culture => devFsPaths.GetMarkdownPath(culture.Code, fileName)),
            DevFileSystemCategory.WorldRules => localizationProvider.GetSupportedLocalizations()
                .Select(culture => devFsPaths.GetWorldRuleMarkdownPath(culture.Code, fileName)),
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported DevFS category.")
        };
}
