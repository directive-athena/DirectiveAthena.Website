// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<FaqContent>>]
internal class FaqContentValidator : AbstractValidator<FaqContent> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public FaqContentValidator(ILocalizationProvider localizationProvider) {
        _localizations = localizationProvider.GetSupportedLocalizations();

        RuleFor(rule => rule.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Some rules have missing Id!");

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
    private bool HasLocalizedQuestions(FaqContent rule)
        => HasLocalizedValues(rule.Question);

    private bool HasLocalizedAnswers(FaqContent rule)
        => HasLocalizedValues(rule.Answer);

    private bool HasLocalizedValues(LocalizedDataHolder values)
        => _localizations.All(localization => !values[localization.Code].IsNullOrWhiteSpace());
}
