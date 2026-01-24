// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthena.Website.Services.FileSystem;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IResourceStorage>]
public  class ResourceStorage(
    ILocalFileStorage fileStorage,
    ILocalizationProvider localizationProvider,
    ILogger<ResourceStorage> logger
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
    public async ValueTask<string?> ReadSharedResxAsync(string locale, CancellationToken ct = default) {
        string path = GetSharedResxPath(locale);
        string? result = await fileStorage.ReadFileAsync(path, ct);
        logger.Debug("Read shared resx {Path}: {Result}.", path, result is null ? "missing" : "ok");
        return result;
    }

    public async ValueTask<bool> WriteSharedResxAsync(string locale, string content, CancellationToken ct = default) {
        string path = GetSharedResxPath(locale);
        bool result = await fileStorage.WriteFileAsync(path, content, ct);
        logger.Information("Write shared resx {Path}: {Result}.", path, result ? "success" : "failure");
        return result;
    }

    public async ValueTask<string?> ReadTagsResxAsync(string locale, CancellationToken ct = default) {
        string path = GetTagsResxPath(locale);
        string? result = await fileStorage.ReadFileAsync(path, ct);
        logger.Debug("Read tags resx {Path}: {Result}.", path, result is null ? "missing" : "ok");
        return result;
    }

    public async ValueTask<bool> WriteTagsResxAsync(string locale, string content, CancellationToken ct = default) {
        string path = GetTagsResxPath(locale);
        bool result = await fileStorage.WriteFileAsync(path, content, ct);
        logger.Information("Write tags resx {Path}: {Result}.", path, result ? "success" : "failure");
        return result;
    }
}
