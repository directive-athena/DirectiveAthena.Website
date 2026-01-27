// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleManager>]
public class WorldRuleManager(
    ILocalizationProvider localizationProvider,
    IContentStorageFactory storageFactory,
    HttpClient http,
    IValidator<IEnumerable<WorldRule>> validator,
    ILogger<WorldRuleManager> logger
) : IWorldRuleManager {
    private readonly IContentStorage _storage = storageFactory.ForCategory(ContentCategory.WorldRules);

    public string GetLocalizedQuestion(WorldRule rule)
        => GetLocalizedValue(rule.Question);

    public string GetLocalizedAnswer(WorldRule rule)
        => GetLocalizedValue(rule.Answer);

    public string GetLocalizedFilePath(WorldRule rule) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return _storage.GetMarkdownContentPath(localization.Code, rule.MarkdownFileName);
    }

    public async Task<string> GetRawMarkdownContentAsync(WorldRule rule, string locale, CancellationToken ct = default) {
        try {
            string path = _storage.GetMarkdownContentPath(locale, rule.MarkdownFileName);
            logger.Debug("Fetching markdown for world rule {Id} at {Path}.", rule.Id, path);
            return await http.GetStringAsync(path, ct);
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch markdown for world rule {Id} ({Locale}).", rule.Id, locale);
            return string.Empty;
        }
    }

    public WorldRule NewRule() {
        var id = Guid.CreateVersion7();
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> questions = locals.ToDictionary(c => c.Code, _ => "New Rule");
        Dictionary<string, string> answers = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Answer here");

        var rule = new WorldRule {
            Id = id,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Question = questions,
            Answer = answers,
            Tags = []
        };
        logger.Information("Created new world rule stub {Id}.", rule.Id);
        return rule;
    }

    public bool Validate(IEnumerable<WorldRule> rules, out string? errorMessage) {
        ValidationResult? result = validator.Validate(rules);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        logger.Warning("World rule validation failed: {Error}.", errorMessage);
        return false;
    }

    public async Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(WorldRule rule, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {rule.Question.GetValueOrDefault(c.Code)}\n\n{rule.Answer.GetValueOrDefault(c.Code)}");

        if (!writeToDisk) {
            logger.Debug("Generated world rule stubs for {Id} without writing to disk.", rule.Id);
            return (stubs, false);
        }

        bool wroteAll = true;
        foreach (KeyValuePair<string, string> stub in stubs) {
            string path = _storage.GetMarkdownDiskPath(stub.Key, rule.MarkdownFileName);
            if (!await _storage.WriteFileAsync(path, stub.Value, ct)) {
                wroteAll = false;
            }
        }

        logger.Information("Generated and wrote world rule stubs for {Id} {Result}.", rule.Id, wroteAll ? "succeeded" : "failed");
        return (stubs, wroteAll);
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        logger.Debug("World rules do not use resx initialization.");
        await Task.CompletedTask;
    }

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }
}
