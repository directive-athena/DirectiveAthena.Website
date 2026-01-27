// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.Writings;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Writings.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<Writing>>>]
public class WritingCollectionValidator : AbstractValidator<IEnumerable<Writing>> {
    public WritingCollectionValidator(IValidator<Writing> articleValidator) {
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
    private static bool HasUniqueIds(IEnumerable<Writing> writings) {
        HashSet<Guid> ids = [];
        return writings.All(article => ids.Add(article.Id));
    }
}
