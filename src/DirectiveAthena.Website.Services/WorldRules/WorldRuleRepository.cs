// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json;
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleRepository>]
public class WorldRuleRepository(
    HttpClient http,
    IDevFileSystemManager devFs,
    ILocalizationProvider localizationProvider,
    IDevFileSystemPaths devFsPaths
) : CachedJsonRepository<WorldRule>(http, devFs), IWorldRuleRepository {

    protected override string IndexPath => "content/world-rules/index.json";
    protected override string WritePath => devFsPaths.GetWorldRulesIndexPath();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<IEnumerable<WorldRule>> GetRulesAsync(CancellationToken ct = default) {
        return await GetAllAsync(ct);
    }

    public async ValueTask<WorldRule?> GetRuleByIdAsync(string id, CancellationToken ct = default) {
        var rules = await GetAllAsync(ct);
        return rules.FirstOrDefault(rule => rule.Id == id);
    }

    public async Task<bool> DeleteAsync(WorldRule rule, IEnumerable<WorldRule> rules, CancellationToken ct = default) {
        if (!devFs.IsLocalhost || !await devFs.HasAccessAsync()) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        bool allDeleted = true;
        foreach (string path in localizationProvider.GetSupportedLocalizations()
            .Select(culture => devFsPaths.GetWorldRuleMarkdownPath(culture.Code, rule.File))) {
            bool success = await devFs.DeleteFileAsync(path);
            if (!success) allDeleted = false;
        }

        if (!allDeleted) return false;

        return await SaveAsync(rules, ct);
    }
}
