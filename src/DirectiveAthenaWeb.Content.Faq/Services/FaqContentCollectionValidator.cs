// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using FluentValidation;

namespace DirectiveAthenaWeb.Content.Faq.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableTransient<IValidator<IEnumerable<FaqContent>>>]
internal class FaqContentCollectionValidator : AbstractValidator<IEnumerable<FaqContent>> {
    public FaqContentCollectionValidator(IValidator<FaqContent> ruleValidator) {
        RuleFor(rules => rules)
            .NotNull();

        RuleForEach(rules => rules)
            .SetValidator(ruleValidator);

        RuleFor(rules => rules)
            .Must(HasUniqueIds)
            .WithMessage("Duplicate IDs found!");
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    private static bool HasUniqueIds(IEnumerable<FaqContent> rules) {
        HashSet<Guid> ids = [];
        return rules.All(rule => ids.Add(rule.Id));
    }

}
