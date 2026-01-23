// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthena.Website.Models;
using FluentValidation;

namespace DirectiveAthena.Website.Services.Articles;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<Article>>]
public sealed class ArticleValidator : AbstractValidator<Article> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public ArticleValidator(ILocalizationProvider localizationProvider) {
        _localizations = localizationProvider.GetSupportedLocalizations();

        RuleFor(article => article.Id)
            .NotEmpty()
            .WithMessage("Some posts have missing Id or File!");

        RuleFor(article => article.File)
            .NotEmpty()
            .WithMessage("Some posts have missing Id or File!");

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
    private bool HasLocalizedTitles(Article article)
        => HasLocalizedValues(article.Title);

    private bool HasLocalizedSummaries(Article article)
        => HasLocalizedValues(article.Summary);

    private bool HasLocalizedValues(Dictionary<string, string> values) {
        foreach (LocalizationInfo localization in _localizations) {
            if (!values.TryGetValue(localization.Code, out string? value) || string.IsNullOrWhiteSpace(value)) {
                return false;
            }
        }

        return true;
    }
}
