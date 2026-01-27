// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWorldRuleManager {
    string GetLocalizedQuestion(WorldRule rule);
    string GetLocalizedAnswer(WorldRule rule);
    string GetLocalizedFilePath(WorldRule rule);
    Task<string> GetRawMarkdownContentAsync(WorldRule rule, string locale, CancellationToken ct = default);

    WorldRule NewRule();
    bool Validate(IEnumerable<WorldRule> rules, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(WorldRule rule, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
