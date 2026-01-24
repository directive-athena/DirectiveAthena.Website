// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IResourceStorage>]
public sealed class ResourceStorage(
    ILocalFileStorage fileStorage,
    ILocalizationProvider localizationProvider
) : IResourceStorage {
    public bool IsLocalhost => fileStorage.IsLocalhost;

    // -----------------------------------------------------------------------------------------------------------------
    // Access
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<bool> HasAccessAsync(CancellationToken ct = default)
        => fileStorage.HasAccessAsync(ct);

    public ValueTask<bool> VerifyPermissionAsync(CancellationToken ct = default)
        => fileStorage.VerifyPermissionAsync(ct);

    // -----------------------------------------------------------------------------------------------------------------
    // Paths
    // -----------------------------------------------------------------------------------------------------------------
    public string GetSharedResxPath(string locale)
        => locale == localizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Shared.resx"
            : $"src/DirectiveAthena.Website/Resources/Shared.{locale}.resx";

    public string GetTagsResxPath(string locale)
        => locale == localizationProvider.DefaultLocalization.Code
            ? "src/DirectiveAthena.Website/Resources/Tags.resx"
            : $"src/DirectiveAthena.Website/Resources/Tags.{locale}.resx";

    // -----------------------------------------------------------------------------------------------------------------
    // Resource Ops
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<string?> ReadSharedResxAsync(string locale, CancellationToken ct = default)
        => fileStorage.ReadFileAsync(GetSharedResxPath(locale), ct);

    public ValueTask<bool> WriteSharedResxAsync(string locale, string content, CancellationToken ct = default)
        => fileStorage.WriteFileAsync(GetSharedResxPath(locale), content, ct);

    public ValueTask<string?> ReadTagsResxAsync(string locale, CancellationToken ct = default)
        => fileStorage.ReadFileAsync(GetTagsResxPath(locale), ct);

    public ValueTask<bool> WriteTagsResxAsync(string locale, string content, CancellationToken ct = default)
        => fileStorage.WriteFileAsync(GetTagsResxPath(locale), content, ct);
}
