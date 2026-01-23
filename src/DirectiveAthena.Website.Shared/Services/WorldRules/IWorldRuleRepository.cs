// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWorldRuleRepository {
    ValueTask<IEnumerable<WorldRule>> GetRulesAsync(CancellationToken ct = default);
    ValueTask<WorldRule?> GetRuleByIdAsync(string id, CancellationToken ct = default);
    string AsJsonString(IEnumerable<WorldRule> rules);
    Task<bool> SaveAsync(IEnumerable<WorldRule> rules, CancellationToken ct = default);
    Task<bool> DeleteAsync(WorldRule rule, IEnumerable<WorldRule> rules, CancellationToken ct = default);
}
