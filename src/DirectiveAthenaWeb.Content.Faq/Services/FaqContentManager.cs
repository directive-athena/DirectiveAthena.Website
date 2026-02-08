// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.R2Storage;
using Microsoft.Extensions.Logging;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IFaqContentManager>]
[InjectableScoped<IContentManager<FaqContent>>]
internal class FaqContentManager(
    ILocalizationProvider localizationProvider,
    IR2Storage<FaqContent> storage,
    ILogger<ContentManagerBase<FaqContent>> logger
) : ContentManagerBase<FaqContent>(localizationProvider, storage, logger), IFaqContentManager {
    private readonly ILogger<ContentManagerBase<FaqContent>> _logger = logger;
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public string GetLocalizedQuestion(FaqContent rule)
        => GetLocalizedValue(rule.Question);

    public string GetLocalizedAnswer(FaqContent rule)
        => GetLocalizedValue(rule.Answer);
    
    public override FaqContent Create(Guid id = default, string? internalTitle = null) {
        if (id == Guid.Empty) id = Guid.CreateVersion7();
        DateTime now = DateTime.UtcNow;
        IReadOnlyCollection<LocalizationInfo> locals = LocalizationProvider.GetSupportedLocalizations();
        
        var rule = new FaqContent {
            Id = id,
            Question = LocalizedDataHolder.FromDictionary(locals.ToDictionary(c => c.Code, _ => "New Rule")),
            Answer = LocalizedDataHolder.FromDictionary(locals.ToDictionary(c => c.Code, c => $"{c.DisplayName} - Answer here")),
            Tags = [
            ],
            CreatedAt = now,
            LastModifiedAt = now,
            InternalTitle = internalTitle ?? string.Empty,
            Author = "Anna Sas"
        };
        _logger.Information("Created new world rule stub {Id}.", rule.Id);
        return rule;
    }
}
