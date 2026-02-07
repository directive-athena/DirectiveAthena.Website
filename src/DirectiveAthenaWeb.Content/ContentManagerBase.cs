// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentManagerBase<TContent>(
    ILocalizationProvider localizationProvider,
    IR2Storage<TContent> storage,
    ILogger<ContentManagerBase<TContent>> logger
) : IContentManager<TContent> where TContent : ContentBase, IContent {
    protected ILocalizationProvider LocalizationProvider { get; } = localizationProvider;
    protected IR2Storage<TContent> Storage { get; } = storage;
    protected ILogger<ContentManagerBase<TContent>> Logger { get; } = logger;
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public abstract TContent Create(Guid id = default, string? internalTitle = null);

    public async Task<string> GetMarkdownContentAsync(TContent rule, string locale, CancellationToken ct = default) {
        try {
            string path = Storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            Logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await Storage.ReadFileAsync(path, ct) ?? string.Empty;
        }
        catch (Exception ex) {
            Logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return string.Empty;
        }
    }
    
    public async Task<bool> WriteMarkdownContentAsync(TContent rule, string locale, string content, CancellationToken ct = default) {
        try {
            string path = Storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            Logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await Storage.WriteFileAsync(path, content, ct);
        }
        catch (Exception ex) {
            Logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return false;
        }
    }
    
    public virtual string GetLocalizedTitle(TContent content)
        => GetLocalizedValue(content.LocalizedTitles);

    public virtual string GetLocalizedSummary(TContent content)
        => GetLocalizedValue(content.LocalizedSummaries);

    protected string GetLocalizedValue(LocalizedDataHolder values) {
        LocalizationInfo localization = LocalizationProvider.GetCurrentLocalization();
        string code = localization.Code;
        string fallback = LocalizationProvider.DefaultLocalization.Code;
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!values.TryGetWithFallback(code, fallback, out string? value)) return string.Empty;
        return value;
    }
}
