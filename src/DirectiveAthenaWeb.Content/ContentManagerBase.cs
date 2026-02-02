// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentManagerBase<TContent>(IContentStorageFactory storageFactory, ILogger<ContentManagerBase<TContent>> logger) : IContentManager<TContent> where TContent : ContentBase, IContent {
    protected readonly IContentStorage Storage = storageFactory.ForCategory<TContent>();

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public async Task<string> GetMarkdownContentAsync(TContent rule, string locale, CancellationToken ct = default) {
        try {
            string path = Storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await Storage.ReadFileAsync(path, ct) ?? string.Empty;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return string.Empty;
        }
    }
    
    public async Task<bool> WriteMarkdownContentAsync(TContent rule, string locale, string content, CancellationToken ct = default) {
        try {
            string path = Storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await Storage.WriteFileAsync(path, content, ct);
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return false;
        }
    }
}
