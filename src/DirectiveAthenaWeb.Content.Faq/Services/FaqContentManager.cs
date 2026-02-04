// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
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
    IServiceProvider provider
) : ContentManagerBase<FaqContent>(provider), IFaqContentManager {

    public string GetLocalizedQuestion(FaqContent rule)
        => GetLocalizedValue(rule.Question);

    public string GetLocalizedAnswer(FaqContent rule)
        => GetLocalizedValue(rule.Answer);
    
    public override FaqContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = localizationProvider.GetSupportedLocalizations();

        Dictionary<string, string> questions = locals.ToDictionary(c => c.Code, _ => "New Rule");
        Dictionary<string, string> answers = locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Answer here");

        var rule = new FaqContent {
            Id = id,
            Question = questions,
            Answer = answers,
            Tags = [
            ],
            CreatedAt = now,
            LastModifiedAt = now,
            InternalTitle = internalTitle ?? string.Empty
        };
        Logger.Information("Created new world rule stub {Id}.", rule.Id);
        return rule;
    }

    private string GetLocalizedValue(Dictionary<string, string> values) {
        LocalizationInfo localization = localizationProvider.GetCurrentLocalization();
        return !values.TryGetValue(localization.Code, out string? value)
            ? values.GetValueOrDefault(localizationProvider.DefaultLocalization.Code, string.Empty)
            : value;
    }
}
