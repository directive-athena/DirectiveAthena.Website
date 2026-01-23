// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Net.Http.Json;
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
) : IWorldRuleRepository {
    private readonly SemaphoreSlim _lock = new(1, 1);
    private WorldRule[]? _rules;

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async ValueTask<IEnumerable<WorldRule>> GetRulesAsync(CancellationToken ct = default) {
        if (_rules is not null) return _rules;

        await _lock.WaitAsync(ct);
        try {
            if (_rules is not null) return _rules;

            _rules = await http.GetFromJsonAsync<WorldRule[]>("content/world-rules/index.json", ct);
            _rules ??= [];
        }
        catch {
            _rules = [];
        }
        finally {
            _lock.Release();
        }

        return _rules;
    }

    public async ValueTask<WorldRule?> GetRuleByIdAsync(string id, CancellationToken ct = default) {
        IEnumerable<WorldRule> rules = await GetRulesAsync(ct);
        return rules.FirstOrDefault(rule => rule.Id == id);
    }

    public string AsJsonString(IEnumerable<WorldRule> rules)
        => JsonSerializer.Serialize(rules, Options);

    public async Task<bool> SaveAsync(IEnumerable<WorldRule> rules, CancellationToken ct = default) {
        if (!devFs.IsLocalhost) return false;
        if (!await devFs.VerifyPermissionAsync()) return false;

        string json = AsJsonString(rules);
        return await devFs.WriteFileAsync(devFsPaths.GetWorldRulesIndexPath(), json);
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
