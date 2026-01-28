// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Note.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<NoteContent>>>]
public class NoteContentCollectionValidator : AbstractValidator<IEnumerable<NoteContent>> {
    public NoteContentCollectionValidator(IValidator<NoteContent> articleValidator) {
        RuleFor(notes => notes)
            .NotNull();

        RuleForEach(notes => notes)
            .SetValidator(articleValidator);

        RuleFor(notes => notes)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<NoteContent> notes) {
        HashSet<Guid> ids = [];
        return notes.All(article => ids.Add(article.Id));
    }
}
