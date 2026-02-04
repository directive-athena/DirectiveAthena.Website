// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.R2Storage;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentManagerBase<TContent>(
    IR2Storage<TContent> storage,
    IValidator<TContent> singleValidator,
    IValidator<IEnumerable<TContent>> multipleValidator,
    ILogger<ContentManagerBase<TContent>> logger
) : IContentManager<TContent> where TContent : ContentBase, IContent {
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public abstract TContent Create(Guid id = default, string? internalTitle = null);

    public bool Validate(TContent rule, out string? errorMessage) {
        ValidationResult? result = singleValidator.Validate(rule);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }
    
    public bool Validate(IEnumerable<TContent> rules, out string? errorMessage) {
        ValidationResult? result = multipleValidator.Validate(rules);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }

    public async Task<string> GetMarkdownContentAsync(TContent rule, string locale, CancellationToken ct = default) {
        try {
            string path = storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await storage.ReadFileAsync(path, ct) ?? string.Empty;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return string.Empty;
        }
    }
    
    public async Task<bool> WriteMarkdownContentAsync(TContent rule, string locale, string content, CancellationToken ct = default) {
        try {
            string path = storage.GetMarkdownDiskPath(locale, rule.MarkdownFileName);
            logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await storage.WriteFileAsync(path, content, ct);
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return false;
        }
    }
}
