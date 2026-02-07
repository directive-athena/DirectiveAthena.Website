// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Localization;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<NoteContent>>]
internal class NoteContentValidator : AbstractValidator<NoteContent> {
    private readonly IReadOnlyCollection<LocalizationInfo> _localizations;

    public NoteContentValidator(ILocalizationProvider localizationProvider) {
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
    private bool HasLocalizedTitles(NoteContent article)
        => HasLocalizedValues(article.LocalizedTitles);

    private bool HasLocalizedSummaries(NoteContent article)
        => HasLocalizedValues(article.LocalizedSummaries);

    private bool HasLocalizedValues(LocalizedDataHolder values) {
        if (values.Count == 0) return false;
        
        foreach (LocalizationInfo localization in _localizations) {
            if (!values.TryGet(localization.Code, out string? value)) return false;
            if (value.IsNullOrWhiteSpace()) return false;
        }

        return true;
    }
}
