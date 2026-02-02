// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Content;
using DirectiveAthenaWeb.Services.ContentStorage;
using DirectiveAthenaWeb.Services.Localization;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IFaqContentManager>]
[InjectableScoped<IContentManager<FaqContent>>]
internal class FaqContentManager(
    ILocalizationProvider localizationProvider,
    IContentStorageFactory storageFactory,
    ILogger<FaqContentManager> logger
) : ContentManagerBase<FaqContent>(storageFactory, logger), IFaqContentManager {

    public string GetLocalizedQuestion(FaqContent rule)
        => GetLocalizedValue(rule.Question);

    public string GetLocalizedAnswer(FaqContent rule)
        => GetLocalizedValue(rule.Answer);

    public string GetLocalizedFilePath(FaqContent rule) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return Storage.GetMarkdownContentPath(localization.Code, rule.MarkdownFileName);
    }
    
    public override FaqContent Create(Guid id = default) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> questions = locals.ToDictionary(c => c.Code, _ => "New Rule");
        Dictionary<string, string> answers = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Answer here");

        var rule = new FaqContent {
            Id = id,
            Question = questions,
            Answer = answers,
            Tags = [],
            CreatedAt = now,
            LastModifiedAt = now
        };
        logger.Information("Created new world rule stub {Id}.", rule.Id);
        return rule;
    }

    public async Task<(Dictionary<string, string> Stubs, bool WroteAll)> GenerateStubsAsync(FaqContent rule, bool writeToDisk = false, CancellationToken ct = default) {
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
            string path = Storage.GetMarkdownDiskPath(stub.Key, rule.MarkdownFileName);
            if (!await Storage.WriteFileAsync(path, stub.Value, ct)) {
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
