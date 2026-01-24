// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWorldRuleRepository : ICachedJsonRepository<WorldRule> {
    ValueTask<IEnumerable<WorldRule>> GetRulesAsync(CancellationToken ct = default);
    ValueTask<WorldRule?> GetRuleByIdAsync(string id, CancellationToken ct = default);
    Task<bool> DeleteAsync(WorldRule rule, IEnumerable<WorldRule> rules, CancellationToken ct = default);
}
