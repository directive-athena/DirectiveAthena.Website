// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Writings.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<WritingContent>>>]
public class WritingContentCollectionValidator : AbstractValidator<IEnumerable<WritingContent>> {
    public WritingContentCollectionValidator(IValidator<WritingContent> articleValidator) {
        RuleFor(writings => writings)
            .NotNull();

        RuleForEach(writings => writings)
            .SetValidator(articleValidator);

        RuleFor(writings => writings)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<WritingContent> writings) {
        HashSet<Guid> ids = [];
        return writings.All(article => ids.Add(article.Id));
    }
}
