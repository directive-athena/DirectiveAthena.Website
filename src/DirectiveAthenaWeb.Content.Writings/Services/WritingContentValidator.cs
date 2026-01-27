// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Writings.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<WritingContent>>]
public class WritingContentValidator : AbstractValidator<WritingContent> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public WritingContentValidator(ILocalizationProvider localizationProvider) {
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
    private bool HasLocalizedTitles(WritingContent article)
        => HasLocalizedValues(article.Title);

    private bool HasLocalizedSummaries(WritingContent article)
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
