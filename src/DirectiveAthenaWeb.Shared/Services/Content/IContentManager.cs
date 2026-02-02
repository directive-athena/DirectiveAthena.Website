// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentManager<in TContent> where TContent : IContent {
    Task<string> GetMarkdownContentAsync(TContent rule, string locale, CancellationToken ct = default);
    Task<bool> WriteMarkdownContentAsync(TContent rule, string locale, string content, CancellationToken ct = default);
}
