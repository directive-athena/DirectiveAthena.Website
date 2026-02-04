// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.R2Storage;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public abstract class ContentManagerBase<TContent>(IServiceProvider provider) : IContentManager<TContent> where TContent : ContentBase, IContent {
    protected ILogger<ContentManagerBase<TContent>> Logger { get; } = provider.GetRequiredService<ILogger<ContentManagerBase<TContent>>>();
    protected readonly IContentStorage Storage = provider.GetRequiredService<IContentStorageFactory>().ForCategory<TContent>();

    private IValidator<TContent> SingleValidator { get; } = provider.GetRequiredService<IValidator<TContent>>();
    private IValidator<IEnumerable<TContent>> MultipleValidator { get; } = provider.GetRequiredService<IValidator<IEnumerable<TContent>>>();
    
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
        Logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }
    
    public bool Validate(IEnumerable<TContent> rules, out string? errorMessage) {
        ValidationResult? result = MultipleValidator.Validate(rules);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        Logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }

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
}
