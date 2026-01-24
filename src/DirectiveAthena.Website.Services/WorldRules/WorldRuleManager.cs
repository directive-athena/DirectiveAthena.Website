// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthena.Website.Services.Localization;
using FluentValidation;
using FluentValidation.Results;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IWorldRuleManager>]
public class WorldRuleManager(
    ILocalizationProvider localizationProvider,
    IDevFileSystemManager devFs,
    HttpClient http,
    IValidator<IEnumerable<WorldRule>> validator,
    IDevFileSystemPaths devFsPaths
) : IWorldRuleManager {
    public string GetLocalizedQuestion(WorldRule rule)
        => GetLocalizedValue(rule.Question);

    public string GetLocalizedAnswer(WorldRule rule)
        => GetLocalizedValue(rule.Answer);

    public string GetLocalizedFilePath(WorldRule rule) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return $"content/world-rules/{localization.Code}/{rule.MarkdownFileName}";
    }

    public async Task<string> GetRawMarkdownContentAsync(WorldRule rule, string locale, CancellationToken ct = default) {
        try {
            return await http.GetStringAsync($"content/world-rules/{locale}/{rule.MarkdownFileName}", ct);
        }
        catch {
            return string.Empty;
        }
    }

    public WorldRule NewRule() {
        var id = Guid.CreateVersion7();
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> questions = locals.ToDictionary(c => c.Code, _ => "New Rule");
        Dictionary<string, string> answers = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Answer here");

        return new WorldRule {
            Id = id,
            Date = DateTime.Now.ToString("yyyy-MM-dd"),
            Question = questions,
            Answer = answers,
            Tags = []
        };
    }

    public bool Validate(IEnumerable<WorldRule> rules, out string? errorMessage) {
        ValidationResult? result = validator.Validate(rules);
        if (result.IsValid) {
            errorMessage = null;
            return true;
        }

        errorMessage = result.Errors.First().ErrorMessage;
        return false;
    }

    public async Task<Dictionary<string, string>> GenerateStubsAsync(WorldRule rule, bool writeToDisk = false, CancellationToken ct = default) {
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();
        Dictionary<string, string> stubs = locals.ToDictionary(
            c => c.Code,
            c => $"# {rule.Question.GetValueOrDefault(c.Code)}\n\n{rule.Answer.GetValueOrDefault(c.Code)}");

        if (!writeToDisk || !devFs.IsLocalhost || !await devFs.HasAccessAsync() || !await devFs.VerifyPermissionAsync()) return stubs;

        foreach (KeyValuePair<string, string> stub in stubs) {
            await devFs.WriteFileAsync(devFsPaths.GetWorldRuleMarkdownPath(stub.Key, rule.MarkdownFileName), stub.Value);
        }

        return stubs;
    }

    public async Task EnsureResxAsync(CancellationToken ct = default) {
        await Task.CompletedTask;
    }

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }
}
