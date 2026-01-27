// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.WorldFaq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IWorldFaqManager {
    string GetLocalizedQuestion(WorldFaq rule);
    string GetLocalizedAnswer(WorldFaq rule);
    string GetLocalizedFilePath(WorldFaq rule);
    Task<string> GetRawMarkdownContentAsync(WorldFaq rule, string locale, CancellationToken ct = default);

    WorldFaq NewRule();
    bool Validate(IEnumerable<WorldFaq> rules, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(WorldFaq rule, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
