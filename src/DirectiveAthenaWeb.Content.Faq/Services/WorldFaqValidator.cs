// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using DirectiveAthenaWeb.Services.WorldFaq;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<WorldFaq>>]
public class WorldFaqValidator : AbstractValidator<WorldFaq> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public WorldFaqValidator(ILocalizationProvider localizationProvider) {
        _localizations = localizationProvider.GetSupportedLocalizations();

        RuleFor(rule => rule.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Some rules have missing Id!");

        RuleFor(rule => rule.Date)
            .NotEmpty()
            .WithMessage("Some rules have missing dates!");

        RuleFor(rule => rule)
            .Must(HasLocalizedQuestions)
            .WithMessage("Some rules have missing questions for one or more cultures!");

        RuleFor(rule => rule)
            .Must(HasLocalizedAnswers)
            .WithMessage("Some rules have missing answers for one or more cultures!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private bool HasLocalizedQuestions(WorldFaq rule)
        => HasLocalizedValues(rule.Question);

    private bool HasLocalizedAnswers(WorldFaq rule)
        => HasLocalizedValues(rule.Answer);

    private bool HasLocalizedValues(Dictionary<string, string> values) {
        foreach (LocalizationInfo localization in _localizations) {
            if (!values.TryGetValue(localization.Code, out string? value) || string.IsNullOrWhiteSpace(value)) {
                return false;
            }
        }

        return true;
    }
}
