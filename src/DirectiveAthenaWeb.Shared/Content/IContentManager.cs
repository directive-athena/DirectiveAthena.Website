// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentManager<TContent> where TContent : IContent {
    TContent Create(Guid id = default, string? internalTitle = null);
    
    bool Validate(TContent rule, out string? errorMessage);
    bool Validate(IEnumerable<TContent> rules, out string? errorMessage);
    
    Task<string> GetMarkdownContentAsync(TContent rule, string locale, CancellationToken ct = default);
    Task<bool> WriteMarkdownContentAsync(TContent rule, string locale, string content, CancellationToken ct = default);
    
    string GetLocalizedTitle(TContent content);
    string GetLocalizedSummary(TContent content);
}
