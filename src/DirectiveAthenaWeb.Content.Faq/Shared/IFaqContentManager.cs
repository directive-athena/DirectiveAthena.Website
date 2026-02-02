// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;

namespace DirectiveAthenaWeb.Content.Faq;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IFaqContentManager : IContentManager<FaqContent> {
    string GetLocalizedQuestion(FaqContent rule);
    string GetLocalizedAnswer(FaqContent rule);
    string GetLocalizedFilePath(FaqContent rule);
    
    
    Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(FaqContent rule, bool writeToDisk = false, CancellationToken ct = default);
    Task EnsureResxAsync(CancellationToken ct = default);
}
