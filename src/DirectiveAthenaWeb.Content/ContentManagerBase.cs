// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentManagerBase<TContent>(
    IContentStorageFactory storageFactory,
    ILogger<ContentManagerBase<TContent>> logger
) : IContentManager<TContent> where TContent : ContentBase, IContent {
    protected readonly IContentStorage Storage = storageFactory.ForCategory<TContent>();
    
    [Inject] public IValidator<TContent> SingleValidator { get; set; } = null!;
    [Inject] public IValidator<IEnumerable<TContent>> MultipleValidator { get; set; } = null!;
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public abstract TContent Create(Guid id = default, string? internalTitle = null);

    public bool Validate(TContent rule, out string? errorMessage) {
        ValidationResult? result = SingleValidator.Validate(rule);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }
    
    public bool Validate(IEnumerable<TContent> rules, out string? errorMessage) {
        ValidationResult? result = MultipleValidator.Validate(rules);
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
