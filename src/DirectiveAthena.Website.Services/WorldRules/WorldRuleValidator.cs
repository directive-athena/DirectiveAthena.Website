// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Services.Localization;
using FluentValidation;

namespace DirectiveAthena.Website.Services.WorldRules;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<WorldRule>>]
public sealed class WorldRuleValidator : AbstractValidator<WorldRule> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public WorldRuleValidator(ILocalizationProvider localizationProvider) {
        _localizations = localizationProvider.GetSupportedLocalizations();

        RuleFor(rule => rule.Id)
            .NotEmpty()
            .WithMessage("Some rules have missing Id or File!");

        RuleFor(rule => rule.File)
            .NotEmpty()
            .WithMessage("Some rules have missing Id or File!");

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
    private bool HasLocalizedQuestions(WorldRule rule)
        => HasLocalizedValues(rule.Question);

    private bool HasLocalizedAnswers(WorldRule rule)
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
