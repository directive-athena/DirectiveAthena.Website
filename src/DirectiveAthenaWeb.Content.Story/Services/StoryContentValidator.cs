// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Story.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<StoryContent>>]
public class StoryContentValidator : AbstractValidator<StoryContent> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public StoryContentValidator(ILocalizationProvider localizationProvider) {
        _localizations = localizationProvider.GetSupportedLocalizations();

        RuleFor(article => article.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Some posts have missing Id!");

        RuleFor(article => article)
            .Must(HasLocalizedTitles)
            .WithMessage("Some posts have missing titles for one or more cultures!");

        RuleFor(article => article)
            .Must(HasLocalizedSummaries)
            .WithMessage("Some posts have missing summaries for one or more cultures!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private bool HasLocalizedTitles(StoryContent article)
        => HasLocalizedValues(article.Title);

    private bool HasLocalizedSummaries(StoryContent article)
        => HasLocalizedValues(article.Summary);

    private bool HasLocalizedValues(Dictionary<string, string> values) {
        foreach (LocalizationInfo localization in _localizations) {
            if (!values.TryGetValue(localization.Code, out string? value) || value.IsNullOrWhiteSpace()) {
                return false;
            }
        }

        return true;
    }
}
