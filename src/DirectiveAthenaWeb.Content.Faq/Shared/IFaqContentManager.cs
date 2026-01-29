// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IFaqContentManager {
    string GetLocalizedQuestion(FaqContent rule);
    string GetLocalizedAnswer(FaqContent rule);
    string GetLocalizedFilePath(FaqContent rule);
    Task<string> GetRawMarkdownContentAsync(FaqContent rule, string locale, CancellationToken ct = default);

    FaqContent NewRule();
    bool Validate(IEnumerable<FaqContent> rules, out string? errorMessage);
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(FaqContent rule, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
